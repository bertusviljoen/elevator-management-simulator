using Application.Abstractions.Screen;
using Application.Abstractions.Services;
using Application.Services;
using Domain.Common;
using Domain.Elevators;
using Infrastructure.Migrations;
using Infrastructure.Persistence.SeedData;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Extensions;
using Spectre.Console;

namespace Presentation.Screens.Dashboard;

/// <summary> Dashboard screen which displays the current status of the elevators in the building. </summary>
public class DashboardScreen(IServiceProvider serviceProvider) : IScreen<bool>
{
    public async Task<Result<bool>> ShowAsync(CancellationToken token)
    {
        AnsiConsole.Clear();

        var cts = new CancellationTokenSource();
        var exitTask = Task.Run(() =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true).Key;
                    if (key == ConsoleKey.C)
                    {
                        cts.Cancel();
                    }
                }
            }
        });

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                AnsiConsole.Write(
                    new FigletText(" Dashboard")
                        .LeftJustified()
                        .Color(Color.Blue)
                );

                var reload = true;
                var buildingId = ApplicationDbContextSeedData.GetSeedBuildings().First()!.Id;
                Result<IEnumerable<ElevatorItem>> result = null;
                await AnsiConsole.Status()
                    .StartAsync("Retrieving configured elevators...", async ctx =>
                    {
                        var elevatorPoolService = serviceProvider.GetRequiredService<IInMemoryElevatorPoolService>();
                        result =
                            await elevatorPoolService.GetAllElevatorsAsync(buildingId, token);
                    });

                _ = result?.Match(
                    onSuccess: elevators =>
                    {
                        var table = new Table();
                        table.AddColumn("Elevator Number");
                        table.AddColumn("Current Floor");
                        table.AddColumn("Direction");
                        table.AddColumn("Status");
                        table.AddColumn("Type");
                        table.AddColumn("Speed");
                        table.AddColumn("Capacity");
                        foreach (var elevator in elevators.OrderBy(a => a.Number))
                        {
                            table.AddRow(
                                elevator.Number.ToString(),
                                elevator.CurrentFloor.ToString(),
                                elevator.ElevatorDirection.ToString(),
                                elevator.ElevatorStatus.ToString(),
                                elevator.ElevatorType.ToString(),
                                elevator.Speed.ToString(),
                                elevator.Capacity.ToString());
                        }

                        AnsiConsole.Render(table);
                        return true;
                    },
                    onFailure: error =>
                    {
                        var friendlyError = error.Error switch
                        {
                            ValidationError validationError => string.Join(", ",
                                validationError.Errors.Select(e => e.Description)),
                            _ => "An error occurred while retrieving the elevators."
                        };
                        AnsiConsole.MarkupLine($"[red]{friendlyError}[/]");
                        return false;
                    }) ?? false;

                AnsiConsole.WriteLine("Press [green]C[/] to exit or press any key to pause the counter...");
                await AnsiConsole.Status()
                    .StartAsync($"Refreshing elevators in 1 second.... {DateTime.UtcNow.ToLocalTime()}", async ctx =>
                    {
                        await Task.Delay(1000, cts.Token);
                    });

                AnsiConsole.Console.Clear();
            }
        }
        catch (TaskCanceledException ex)
        {
            //we are ignoring this one as this is the user initiated exit
        }
        finally
        {
            AnsiConsole.WriteLine("Exiting dashboard...");
            await cts.CancelAsync();
            await exitTask; // Ensure the background task completes
        }
         
        return true;
    }
}
