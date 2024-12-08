using Application;
using Application.Abstractions.Data;
using Infrastructure;
using Microsoft.Extensions.Hosting;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Domain.Users;

namespace InfrastructureTests.Building;

public class BuildingManagementTests
{
    public BuildingManagementTests()
    {

    }

    [Fact]
    public async Task CantCreateBuildingWithoutUser()
    {
        var host = new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddApplication();
                services.AddInfrastructure(hostContext.Configuration, true);
            })
            .Build();

        var building = new Domain.Buildings.Building()
        {
            Id = Guid.NewGuid(),
            Name = Faker.Company.Name(),
            NumberOfFloors = 3,
        };

        var applicationContext = host.Services.GetRequiredService<IApplicationDbContext>();

        applicationContext.Buildings.Add(building);

        await Assert.ThrowsAsync<DbUpdateException>(() => applicationContext.SaveChangesAsync(CancellationToken.None));


    }
}
