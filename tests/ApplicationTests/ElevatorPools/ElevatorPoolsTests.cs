using Application;
using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Domain.Users;
using Domain.Elevators;
using Infrastructure;

using Infrastructure.Persistence.Database;
using Infrastructure.Persistence.SeedData;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit.Sdk;

namespace ApplicationTests.ElevatorPools;

public class ElevatorPoolsTests
{
    [Fact]
    public void CanDiElevatorPoolService()
    {
        var host = new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddApplication();
                services.AddInfrastructure(hostContext.Configuration,
                    true);
            })
            .Build();

        var elevatorPoolService = host.Services.GetRequiredService<IInMemoryElevatorPoolService>();
        Assert.NotNull(elevatorPoolService);
    }

    [Fact]
    public async Task CanGetAllElevators()
    {
        // Arrange
        var host = new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddApplication();
                services.AddInfrastructure(hostContext.Configuration,
                    true);
            })
            .Build();
        
        // Get the service first since it's a singleton
        var elevatorPoolService = host.Services.GetRequiredService<IInMemoryElevatorPoolService>();

        // Then create a scope for database operations
        using var scope = host.Services.CreateScope();
        await SeedInMemoryDatabase(scope);

        // Act
        var buildingId = ApplicationDbContextSeedData.GetSeedBuildings().First().Id;
        var result = await elevatorPoolService.GetAllElevatorsAsync(buildingId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        var elevators = result.Value.ToList();
        Assert.NotEmpty(elevators);
        Assert.Equal(5, elevators.Count);

        // Verify elevator properties
        foreach (var elevator in elevators)
        {
            Assert.Equal(buildingId, elevator.BuildingId);
            Assert.Equal(ElevatorDirection.None, elevator.ElevatorDirection);
            Assert.Equal(1, elevator.CurrentFloor);
        }
    }

    internal async Task SeedInMemoryDatabase(IServiceScope scope)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        // Clear existing data
        dbContext.Users.RemoveRange(dbContext.Users);
        dbContext.Buildings.RemoveRange(dbContext.Buildings);
        dbContext.Elevators.RemoveRange(dbContext.Elevators);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        // Add seed data
        foreach (User seedUser in ApplicationDbContextSeedData.GetSeedUsers())
        {
            dbContext.Users.Add(seedUser);
        }
        await dbContext.SaveChangesAsync(CancellationToken.None);

        foreach (Domain.Buildings.Building seedBuilding in ApplicationDbContextSeedData.GetSeedBuildings())
        {
            dbContext.Buildings.Add(seedBuilding);
        }
        await dbContext.SaveChangesAsync(CancellationToken.None);

        foreach (Domain.Elevators.Elevator seedElevator in ApplicationDbContextSeedData.GetSeedElevators())
        {
            dbContext.Elevators.Add(seedElevator);
        }
        await dbContext.SaveChangesAsync(CancellationToken.None);
    }
}
