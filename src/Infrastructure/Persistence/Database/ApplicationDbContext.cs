using Application.Abstractions.Data;
using Domain.Buildings;
using Domain.Elevators;
using Domain.Users;
using Infrastructure.Database;
using Infrastructure.Persistence.SeedData;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Database;

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
