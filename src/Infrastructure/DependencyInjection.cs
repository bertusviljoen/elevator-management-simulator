using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;


public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool useInMemoryDatabase = false) =>
        services
            .AddServices()
            .AddDatabase(configuration, useInMemoryDatabase)
            .AddHealthChecks(configuration)
            .AddAuthenticationInternal(configuration)
        ;
    
    /// <summary> Run migrations for the EF Core database context. </summary>
    public static async Task<IHost> RunMigrationsAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            await dbContext.Database.MigrateAsync();
            logger.LogInformation($"Successfully migrated the database");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An error occurred while migrating the database");
            throw;
        }
        return host;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddHostedService<ElevatorSimulationHostedService>();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration, bool useInMemoryDatabase)
    {
        //register interceptors
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        if (useInMemoryDatabase)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options
                    .UseInMemoryDatabase("TestDb")
                    .UseSnakeCaseNamingConvention()
                    .AddInterceptors(services.BuildServiceProvider().GetServices<ISaveChangesInterceptor>())
                    .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                );
        }
        else
        {
            string? connectionString = configuration.GetConnectionString("Database");
            services.AddDbContext<ApplicationDbContext>(
                options => options
                    .UseSqlite(connectionString)
                    .UseSnakeCaseNamingConvention()
                    .AddInterceptors(services.BuildServiceProvider().GetServices<ISaveChangesInterceptor>())
                );
        }

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration.GetConnectionString("Database") != null)
        {
            services
                .AddHealthChecks()
                .AddSqlite(configuration.GetConnectionString("Database")!);
        }

        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenProvider, TokenProvider>();

        return services;
    }

    private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    {
        services.AddAuthorization();

        services.AddScoped<PermissionProvider>();

        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        return services;
    }
}




internal static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal? principal)
    {
        string? userId = principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(userId, out Guid parsedUserId) ?
            parsedUserId :
            throw new ApplicationException("User id is unavailable");
    }
}



internal sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 500000;

    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

    public string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }

    public bool Verify(string password, string passwordHash)
    {
        string[] parts = passwordHash.Split('-');
        byte[] hash = Convert.FromHexString(parts[0]);
        byte[] salt = Convert.FromHexString(parts[1]);

        byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        return CryptographicOperations.FixedTimeEquals(hash, inputHash);
    }
}



internal sealed class TokenProvider(IConfiguration configuration) : ITokenProvider
{
    public string Create(User user)
    {
        string secretKey = configuration["Jwt:Secret"]!;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            ]),
            Expires = DateTime.UtcNow.AddMinutes(configuration.GetValue<int>("Jwt:ExpirationInMinutes")),
            SigningCredentials = credentials,
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"]
        };

        var handler = new JsonWebTokenHandler();

        string token = handler.CreateToken(tokenDescriptor);

        return token;
    }
}




internal sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid UserId =>
        httpContextAccessor
            .HttpContext?
            .User
            .GetUserId() ??
        throw new ApplicationException("User context is unavailable");
}




[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission)
        : base(permission)
    {
    }
}




internal sealed class PermissionAuthorizationHandler(IServiceScopeFactory serviceScopeFactory)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // TODO: You definitely want to reject unauthenticated users here.
        if (context.User is not { Identity.IsAuthenticated: true } or { Identity.IsAuthenticated: false })
        {
            // TODO: Remove this call when you implement the PermissionProvider.GetForUserIdAsync
            context.Succeed(requirement);

            return;
        }

        using IServiceScope scope = serviceScopeFactory.CreateScope();

        PermissionProvider permissionProvider = scope.ServiceProvider.GetRequiredService<PermissionProvider>();

        Guid userId = context.User.GetUserId();

        HashSet<string> permissions = await permissionProvider.GetForUserIdAsync(userId);

        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);

            return;
        }
    }
}




internal sealed class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    private readonly AuthorizationOptions _authorizationOptions;

    public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options)
    {
        _authorizationOptions = options.Value;
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        AuthorizationPolicy? policy = await base.GetPolicyAsync(policyName);

        if (policy is not null)
        {
            return policy;
        }

        AuthorizationPolicy permissionPolicy = new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();

        _authorizationOptions.AddPolicy(policyName, permissionPolicy);

        return permissionPolicy;
    }
}


internal sealed class PermissionProvider
{
    public Task<HashSet<string>> GetForUserIdAsync(Guid userId)
    {
        if (Guid.Empty.Equals(userId))
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }
        // TODO: Here you'll implement your logic to fetch permissions.
        HashSet<string> permissionsSet = [];

        return Task.FromResult(permissionsSet);
    }
}




internal sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }

    public string Permission { get; }
}



/// <summary> Background service for simulating elevator operations. </summary>
public class ElevatorSimulationHostedService(
    ILogger<ElevatorSimulationHostedService> logger,
    IInMemoryElevatorPoolService elevatorPoolService)
    : BackgroundService
{
    private readonly TimeSpan _simulationInterval = TimeSpan.FromSeconds(1);

    //ToDo: This should be configurable
    private readonly Guid _buildingId = ApplicationDbContextSeedData.GetSeedBuildings()!.FirstOrDefault()!.Id;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Elevator simulation service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Get all the elevators based on the building id
                var elevatorsResult = await elevatorPoolService.GetAllElevatorsAsync(_buildingId, stoppingToken);
                if (elevatorsResult.IsFailure)
                {
                    logger.LogWarning("Failed to get elevators: {Error}", elevatorsResult.Error);
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Back off on error
                    continue;
                }

                foreach (var elevator in elevatorsResult.Value.AsParallel())
                {
                    logger.LogInformation(
                        "Simulating elevator {ElevatorId} on floor {Floor}",
                        elevator.Id, elevator.CurrentFloor);
                    
                    var elevatorChanged = true;
                    if (elevator.ElevatorStatus != ElevatorStatus.Active)
                    {
                        continue;
                    }

                    // Simulate elevator movement based on direction
                    switch (elevator.ElevatorDirection)
                    {
                        case ElevatorDirection.Up:
                            elevator.CurrentFloor += elevator.FloorsPerSecond;
                            //check for when speed elevators pass the destination floor
                            if (elevator.ElevatorType == ElevatorType.HighSpeed &&
                                elevator.CurrentFloor > elevator.DestinationFloor)
                            {
                                elevator.CurrentFloor = elevator.DestinationFloor;
                            }
                            break;
                        case ElevatorDirection.Down:
                            elevator.CurrentFloor -= elevator.FloorsPerSecond;
                            //check for when speed elevators pass the destination floor
                            if (elevator.ElevatorType == ElevatorType.HighSpeed &&
                                elevator.CurrentFloor < elevator.DestinationFloor)
                            {
                                elevator.CurrentFloor = elevator.DestinationFloor;
                            }
                            break;
                        case ElevatorDirection.None:
                            {
                                if (elevator.DestinationFloors.Count > 0)
                                {
                                    var destinationFloor = elevator.DestinationFloors.Dequeue();
                                    elevator.ElevatorDirection = destinationFloor > elevator.CurrentFloor
                                        ? ElevatorDirection.Up
                                        : ElevatorDirection.Down;
                                    elevator.DestinationFloor = destinationFloor;
                                    elevator.DoorStatus = ElevatorDoorStatus.Closed;
                                    break;
                                }
                                elevatorChanged = false;
                            }
                            break;
                    }
                    
                    if (elevator.CurrentFloor == elevator.DestinationFloor)
                    {
                        elevator.ElevatorDirection = ElevatorDirection.None;
                        elevator.DoorStatus = ElevatorDoorStatus.Open;
                    }
                    
                    if (elevator.ElevatorDirection == ElevatorDirection.None)
                    {
                        elevator.DoorStatus = ElevatorDoorStatus.Open;
                    }

                    // Update elevator state with new floor
                    if (elevatorChanged)
                    {
                        await elevatorPoolService.UpdateElevatorAsync(
                            elevator, stoppingToken);

                        logger.LogInformation(
                            "Elevator {ElevatorId} moved to floor {Floor}",
                            elevator.Id, elevator.CurrentFloor);
                    }
                }

                await Task.Delay(_simulationInterval, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "An error occurred while simulating elevator movements");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Back off on error
            }
        }
    }

}



internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}




/// <inheritdoc cref="IApplicationDbContext" />
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):
    DbContext(options), IApplicationDbContext
{
    /// <inheritdoc cref="IApplicationDbContext" />
    public DbSet<User> Users { get; init; }
    /// <inheritdoc cref="IApplicationDbContext" />
    public DbSet<Building> Buildings { get; init; }
    /// <inheritdoc cref="IApplicationDbContext" />
    public DbSet<Elevator> Elevators { get; init; }

    /// <inheritdoc cref="DbContext" />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.HasDefaultSchema(Schemas.Default);
        
        modelBuilder.SeedData();
    }

    /// <inheritdoc cref="DbContext" />
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int result = await base.SaveChangesAsync(cancellationToken);
        return result;
    }

}


/// <summary> Static class to hold database schema names. </summary>
internal static class Schemas
{
    /// <summary> The default schema name. </summary>
    public const string Default = "dbo";
}




internal sealed class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.HasKey(b => b.Id);

        builder.HasIndex(b => b.Name).IsUnique();

        builder.Property(b => b.Name)
            .HasMaxLength(100)
            .IsRequired();
        
        //the below relationship reads as follows:
        //a building has relationship with CreatedByUser with a foreign key of CreatedByUserId
        //the relationship is one to many meaning that a building can only have one user that created it
        builder.HasOne(b => b.CreatedByUser)
            .WithMany()
            .HasForeignKey(b => b.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(b => b.UpdatedByUser)
            .WithMany()
            .HasForeignKey(b => b.UpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}




internal sealed class ElevatorConfiguration : IEntityTypeConfiguration<Elevator>
{
    public void Configure(EntityTypeBuilder<Elevator> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Number)
            .IsRequired();
        
        //HasIndex to be unique across building id and number
        builder.HasIndex(e => new { e.BuildingId, e.Number })
            .IsUnique();

        builder.Property(e => e.CurrentFloor)
            .IsRequired();

        builder.Property(e => e.ElevatorDirection)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(e => e.ElevatorStatus)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(e => e.ElevatorType)
            .HasConversion<string>()            
            .IsRequired();

        builder.Property(e => e.FloorsPerSecond)
            .IsRequired();

        builder.Property(e => e.QueueCapacity)
            .IsRequired();

        builder.HasOne(e => e.Building)
            .WithMany()
            .HasForeignKey(e => e.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}




/// <summary> The user entity configuration for the database. </summary>
internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.HasIndex(u => u.Email).IsUnique();
    }
}



/// <summary> Dispatch Domain Events Interceptor </summary>
public class DispatchDomainEventsInterceptor(IMediator mediator) : SaveChangesInterceptor
{
    /// <inheritdoc cref="SaveChangesInterceptor"/> 
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        DispatchDomainEvents(eventData.Context).GetAwaiter().GetResult();
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc cref="SaveChangesInterceptor"/> 
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        await DispatchDomainEvents(eventData.Context);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary> Dispatch domain events via Mediator </summary>
    private async Task DispatchDomainEvents(DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        var domainEvents = context.ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                List<IDomainEvent> domainEvents = entity.DomainEvents;

                entity.ClearDomainEvents();

                return domainEvents;
            })
            .ToList();

        foreach (IDomainEvent domainEvent in domainEvents)
        {
            await mediator.Publish(domainEvent);
        }
    }
}




public static class ApplicationDbContextSeedData
{

    public static List<User> GetSeedUsers()
    {
        //Administrator user to manage the application
        return new List<User>
        {
            new()
            {
                Id = Guid.Parse("31a9cff7-dc59-4135-a762-6e814bab6f9a"),
                FirstName = "Admin",
                LastName = "Joe",
                Email = "admin@building.com",
                PasswordHash =
                    "55BC042899399B562DD4A363FD250A9014C045B900716FCDC074861EB69C344A-B44367BE2D0B037E31AEEE2649199100", //Admin123
            }
        };
    }
    
    public static List<Building> GetSeedBuildings()
    {
        return new List<Building>
        {
            new()
            {
                Id = Guid.Parse("e16e32e7-8db0-4536-b86e-f53e53cd7a0d"),
                Name = "Joe's Building",
                NumberOfFloors = 10,
                CreatedByUserId = GetSeedUsers().FirstOrDefault()!.Id
            }
        };
    }

    public static List<Elevator> GetSeedElevators()
    {
        return new List<Elevator>
        {
            new()
            {
                Id = Guid.Parse("852bb6fa-1831-49ef-a0d9-5bfa5f567841"),
                CurrentFloor = 1,
                Number = 1,
                ElevatorDirection = ElevatorDirection.None,
                ElevatorStatus = ElevatorStatus.Active,
                ElevatorType = ElevatorType.HighSpeed,
                BuildingId = GetSeedBuildings().FirstOrDefault()!.Id,
                CreatedByUserId = GetSeedUsers().FirstOrDefault()!.Id,
                FloorsPerSecond = 5,
                QueueCapacity = 3
            },
            new()
            {
                Id = Guid.Parse("14ef29a8-001e-4b70-93b6-bfdb00237d46"),
                CurrentFloor = 1,
                Number = 2,
                ElevatorDirection = ElevatorDirection.None,
                ElevatorStatus = ElevatorStatus.Active,
                ElevatorType = ElevatorType.Passenger,
                BuildingId = GetSeedBuildings().FirstOrDefault()!.Id,
                CreatedByUserId = GetSeedUsers().FirstOrDefault()!.Id,
                QueueCapacity = 3
            },
            new()
            {
                Id = Guid.Parse("966b1041-ff39-432b-917c-b0a14ddce0bd"),
                CurrentFloor = 1,
                Number = 3,
                ElevatorDirection = ElevatorDirection.None,
                ElevatorStatus = ElevatorStatus.Active,
                ElevatorType = ElevatorType.Passenger,
                BuildingId = GetSeedBuildings().FirstOrDefault()!.Id,
                CreatedByUserId = GetSeedUsers().FirstOrDefault()!.Id,
                QueueCapacity = 3
            },
            new()
            {
                Id = Guid.Parse("b8557436-6472-4ad7-b111-09c8a023c463"),
                CurrentFloor = 1,
                Number = 4,
                ElevatorDirection = ElevatorDirection.None,
                ElevatorStatus = ElevatorStatus.Maintenance,
                ElevatorType = ElevatorType.Passenger,
                BuildingId = GetSeedBuildings().FirstOrDefault()!.Id,
                CreatedByUserId = GetSeedUsers().FirstOrDefault()!.Id,
                QueueCapacity = 3
            },
            new()
            {
                Id = Guid.Parse("bbfbdffa-f7cd-4241-a222-85a733098782"),
                CurrentFloor = 1,
                Number = 5,
                ElevatorDirection = ElevatorDirection.None,
                ElevatorStatus = ElevatorStatus.OutOfService,
                ElevatorType = ElevatorType.Passenger,
                BuildingId = GetSeedBuildings().FirstOrDefault()!.Id,
                CreatedByUserId = GetSeedUsers().FirstOrDefault()!.Id,
                QueueCapacity = 3
            }
        };
    }

    public static void SeedData(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(GetSeedUsers());
        modelBuilder.Entity<Building>().HasData(GetSeedBuildings());
        modelBuilder.Entity<Elevator>().HasData(GetSeedElevators());
    }
}
