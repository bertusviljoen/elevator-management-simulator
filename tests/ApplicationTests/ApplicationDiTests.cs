using Application;
using Application.Abstractions.Services;
using Application.Services.ElevatorSelection;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ApplicationTests;

public class ApplicationDiTests
{
    private readonly IServiceProvider _serviceProvider;

    public ApplicationDiTests()
    {
        IHost host = new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddApplication();
            })
            .Build();
        
        _serviceProvider = host.Services;   
    }
    
    //DI test for Application
    [Fact]
    public void CanResolveElevatorSelectionContext()
    {
        var service = _serviceProvider.GetRequiredService<IElevatorSelectionContext>();
        Assert.NotNull(service);
    }
    

    [Fact]
    public void CanResolveElevatorOrchestratorService()
    {
        var service = _serviceProvider.GetRequiredService<IElevatorOrchestratorService>();
        Assert.NotNull(service);
    }
    
    [Fact]
    public void CanResolveClosestElevatorStrategy()
    {
        var service = _serviceProvider.GetRequiredService<IClosestElevatorStrategy>();
        Assert.NotNull(service);
    }
    
    [Fact]
    public void CanResolveQueueCapacityStrategy()
    {
        var service = _serviceProvider.GetRequiredService<IQueueCapacityStrategy>();
        Assert.NotNull(service);
    }
}
