using Application;
using Application.Abstractions.Data;
using Infrastructure;
using Microsoft.Extensions.Hosting;
using Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace InfrastructureTests.Building;

public class BuildingManagementTests
{
    public BuildingManagementTests()
    {
        
    }
    
    [Fact]
    public Task CantCreateBuildingWithoutUser()
    {
        var host = new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddApplication();
                services.AddInfrastructure(hostContext.Configuration, true);
            })
            .Build();

        var building = new Domain.Entities.Building()
        {
            Id = Guid.NewGuid(),
            Name = Faker.Company.Name(),
            NumberOfFloors = 3
        };
        
        var applicationContext = host.Services.GetRequiredService<IApplicationDbContext>();
        
        applicationContext.Buildings.Add(building);
        
        return Assert.ThrowsAsync<ValidationException>(() => applicationContext.SaveChangesAsync());

    }
}
