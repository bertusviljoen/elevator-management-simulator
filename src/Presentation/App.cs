using Application.Screens;
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
public class App(IServiceProvider serviceProvider) : IHostedService
{
    // This method is called when the host starts
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Display a fancy header
        DisplayHeader();

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
                    if (loginResult.IsSuccess)
                    {
                        AnsiConsole.MarkupLine($"[green]Login successful[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[red]Login failed[/]");
                    }
                    break;
                case MenuSelection.ElevatorControl:
                    var elevatorControlScreen = serviceProvider.GetRequiredService<ElevatorControlScreen>();
                    await elevatorControlScreen.ShowAsync(cancellationToken);
                    break;
            }
            
            AnsiConsole.Clear();
            DisplayHeader();
            menuSelection = await menuScreen.ShowAsync(cancellationToken);
        }
        await StopAsync(cancellationToken);
    }

    private static void DisplayHeader()
    {
        AnsiConsole.Write(
            new FigletText(" Welcome to Elevator Simulator")
                .LeftJustified()
                .Color(Color.Blue)
        );
    }

    // This method is called when the host is shutting down
    public Task StopAsync(CancellationToken cancellationToken)
    {
        AnsiConsole.MarkupLine("[grey]Shutting down...[/]");
        return Task.CompletedTask;
    }
}
