using Application;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Presentation;

public static class Program
{
    public static async Task Main(string[] args)
    {
        // Build a generic host with default configuration
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Add the Serilog logger
                services.AddSerilog();
                
                // Add the application services
                services.AddApplication();
                // Add the infrastructure services
                services.AddInfrastructure(context.Configuration);
                //Add the presentation services
                services.AddScreens(context.Configuration);
                // Register our hosted service (the entry point logic)
                services.AddHostedService<App>();
            })
            .Build();

        await host.RunMigrationsAsync();
        
        // Run the host (this will call StartAsync on IHostedService implementations)
        await host.RunAsync();
    }
}

