using Application.Abstractions.Screen;
using Application.Elevators.Request;
using Domain.Common;
using Infrastructure.Persistence.SeedData;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Presentation.Extensions;
using Spectre.Console;

namespace Presentation.Screens.ElevatorControl;

public class ElevatorControlMultipleRequestScreen(IServiceProvider serviceProvider, 
    ILogger<ElevatorControlMultipleRequestScreen> logger): IScreen<bool>
{
    public async Task<Result<bool>> ShowAsync(CancellationToken token)
    {
        Result<Guid>? result = null;
        var anotherRequest = true;
        while (anotherRequest)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(
                new FigletText("Elevator Control")
                    .LeftJustified()
                    .Color(Color.Blue)
            );

            var floors = AnsiConsole.Prompt(
                new TextPrompt<string>("What floors are you going to? (comma separated)"));
            
            //remove trailing comma
            if (floors.EndsWith(","))
            {
                floors = floors.Remove(floors.Length - 1);
            }

            var buildingId = ApplicationDbContextSeedData.GetSeedBuildings().First()!.Id;
            await AnsiConsole.Status()
                .StartAsync("Requesting elevators...", async ctx =>
                {
                    logger.LogInformation("Requesting elevators for building {BuildingId}", buildingId);
                    logger.LogInformation("Floors requested: {Floors}", floors);
                    var floorRequests = floors.Split(',').Select(int.Parse).ToList();
                    foreach (int floorRequest in floorRequests)
                    {
                        var scope = serviceProvider.CreateScope();
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                        var request = new RequestElevatorCommand(buildingId, floorRequest);
                        result = await mediator.Send(request, token);                        
                    }
                });

            anotherRequest = result?.Match(
                onSuccess: () =>
                {
                    logger.LogInformation("Elevators requested successfully");
                    AnsiConsole.MarkupLine("[green]Elevators requested successfully[/]");
                    return AnsiConsole.Confirm("Do you want to request more elevators?");
                },
                onFailure: error =>
                {
                    var friendlyError = GetErrorMessage(error);
                    AnsiConsole.MarkupLine($"[red]{friendlyError}[/]");
                    logger.LogError("Error requesting elevators: {Error}", friendlyError);
                    return AnsiConsole.Confirm("Do you want to try again?");
                }) ?? false;
        }
        return true;
    }

    private string GetErrorMessage(Result error)
    {
        if (error.Error is ValidationError validationError)
        {
            return string.Join(", ", validationError.Errors.Select(e => e.Description));
        }
        return error.Error.Description;
    }
}
