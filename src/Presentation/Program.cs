using Application;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using DependencyInjection = Infrastructure.DependencyInjection;

namespace Presentation;

public static class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            //Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}:{Message:lj}:{Exception}{NewLine}")
                .CreateLogger();

            // Build a generic host with default configuration
            var host = Host.CreateDefaultBuilder(args)
                .UseSerilog() // Use Serilog as the logging provider
                .ConfigureServices((context, services) =>
                {
                    // Add the application services
                    services.AddApplication();
                    // Add the infrastructure services
                    services.AddInfrastructure(context.Configuration);
                    //Add the presentation services
                    services.AddScreens(context.Configuration);
                    // Register our hosted service (the entry point logic)
                    services.AddHostedService<App>();
                })
                .ConfigureAppConfiguration((context, builder) => builder.AddUserSecrets<App>())
                .Build();

            await host.RunMigrationsAsync();

            await host.RunAsync();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Console.WriteLine("Thank you! Bye bye");
            await Log.CloseAndFlushAsync();
        }
    }
}
