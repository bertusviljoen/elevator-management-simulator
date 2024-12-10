using Application.Abstractions.Data;
using Application.Screens;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Presentation.Screens;
using Presentation.Screens.Dashboard;
using Presentation.Screens.ElevatorControl;
using Spectre.Console;

namespace Presentation;

// A hosted service that can be run by the Host
// This could be replaced by more complex logic such as background tasks, 
// scheduled jobs, or other application logic
public class App(IServiceProvider serviceProvider,IHostApplicationLifetime applicationLifetime) : IHostedService
{
    private string _buildingName = string.Empty;
    // This method is called when the host starts
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        //Get building name
        var applicationContext = serviceProvider.GetRequiredService<IApplicationDbContext>();
        var defaultBuilding = await
            applicationContext.Buildings.FirstOrDefaultAsync(a => a.IsDefault, cancellationToken: cancellationToken);

        if (defaultBuilding != null)
        {
            _buildingName = defaultBuilding.Name;
        }
        
        // Display a fancy header
        DisplayHeader(_buildingName);

        var menuScreen = serviceProvider.GetRequiredService<MenuScreen>();
        var menuSelection = await menuScreen.ShowAsync(cancellationToken);
        
        while (menuSelection.Value != MenuSelection.Exit)
        {
            switch (menuSelection.Value)
            {
                case MenuSelection.Dashboard:
                    var dashboardScreen = serviceProvider.GetRequiredService<DashboardScreen>();
                    await dashboardScreen.ShowAsync(cancellationToken);
                    break;
                case MenuSelection.Login:
                    var loginScreen = serviceProvider.GetRequiredService<LoginScreen>();
                    var loginResult = await loginScreen.ShowAsync(cancellationToken);
                    if (loginResult.IsSuccess && !string.IsNullOrEmpty(loginResult.Value))
                    {
                        var configurationMenu = serviceProvider.GetRequiredService<ConfigurationMenu>();
                        await configurationMenu.ShowAsync(cancellationToken);
                    }
                    break;
                case MenuSelection.ElevatorControl:
                    var elevatorControlScreen = serviceProvider.GetRequiredService<ElevatorControlScreen>();
                    await elevatorControlScreen.ShowAsync(cancellationToken);
                    await serviceProvider.GetRequiredService<DashboardScreen>().ShowAsync(cancellationToken);
                    break;
                case MenuSelection.MultiElevatorControl:
                    var multiElevatorControlScreen = serviceProvider.GetRequiredService<ElevatorControlMultipleRequestScreen>();
                    await multiElevatorControlScreen.ShowAsync(cancellationToken);
                    await serviceProvider.GetRequiredService<DashboardScreen>().ShowAsync(cancellationToken);
                    break;
            }
            
            AnsiConsole.Clear();
            DisplayHeader(_buildingName);
            menuSelection = await menuScreen.ShowAsync(cancellationToken);
        }
        await StopAsync(cancellationToken);
    }

    private static void DisplayHeader(string buildingName)
    {
        //check if building name is empty
        var name = string.IsNullOrEmpty(buildingName) ? "" : $"{buildingName}'s";
        AnsiConsole.Write(
            new FigletText($" Welcome to {name} Elevator Simulator")
                .LeftJustified()
                .Color(Color.Blue)
        );
    }

    // This method is called when the host is shutting down
    public Task StopAsync(CancellationToken cancellationToken)
    {
        AnsiConsole.MarkupLine("[grey]Shutting down App...[/]");
        applicationLifetime.StopApplication();
        return Task.CompletedTask;
    }
}
