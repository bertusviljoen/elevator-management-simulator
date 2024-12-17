using Application;
using Infrastructure;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ApplicationTests.Buildings;

public class BuildingTests
{
    [Fact]
    public async Task CanCreateBuildingWithCommand()
    {
        var host = new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddApplication();
                services.AddInfrastructure(hostContext.Configuration,
                    true);
            })
            .Build();
        
        var mediator = host.Services.GetRequiredService<IMediator>();

        var result = await mediator.Send(new CreateBuildingCommand(
            Faker.Company.Name(), 1));
        
        Assert.NotNull(result);
    }
}
