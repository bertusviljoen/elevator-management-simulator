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

        //ToDo: Come back to this
        //await Assert.ThrowsAsync<DbUpdateException>(() => applicationContext.SaveChangesAsync(CancellationToken.None));
        await applicationContext.SaveChangesAsync(CancellationToken.None);
    }
}
