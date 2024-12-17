using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
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



/// <summary> Contains service collection extension methods for registering screens. </summary>
public static class DependencyInjections
{
    /// <summary> Registers the screens. </summary>
    public static IServiceCollection AddScreens(
        this IServiceCollection services,
        IConfiguration configuration)
    {
         services.AddTransient<MenuScreen>();
         services.AddTransient<RegisterScreen>();
         services.AddTransient<LoginScreen>();
         services.AddTransient<DashboardScreen>();
         services.AddTransient<ElevatorControlScreen>();
         services.AddTransient<ElevatorControlMultipleRequestScreen>();
         services.AddTransient<ConfigurationMenu>();
         return services;
    }
    

}



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




/// <summary> Extension methods for working with <see cref="Result"/> and <see cref="Result{T}"/>. </summary>
public static class ResultExtensions
{
    /// <summary> Method to match the result of a <see cref="Result"/> and execute the appropriate action. </summary>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">The action to execute if the result is successful.</param>
    /// <param name="onFailure">The action to execute if the result is a failure.</param>
    /// <typeparam name="TOut">The type of the output.</typeparam>
    /// <returns>The output of the executed action.</returns>
    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        Func<Result, TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess() : onFailure(result);
    }

    /// <summary> Method to match the result of a <see cref="Result{T}"/> and execute the appropriate action. </summary>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">The action to execute if the result is successful.</param>
    /// <param name="onFailure">The action to execute if the result is a failure.</param>
    /// <para name="TIn">The type of the input.</para>
    /// <para name="TOut">The type of the output.</para>
    /// <returns>The output of the executed action.</returns>
    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Result<TIn>, TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess(result.Value) : onFailure(result);
    }
}



public class LoginScreen(IMediator mediator) : IScreen<string>
{
    public async Task<Result<string>> ShowAsync(CancellationToken token)
    {
        Result<string>? loginResult = null;
        var tryAgain = true;
        
        while (tryAgain)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(
                new FigletText("User Login")
                    .LeftJustified()
                    .Color(Color.Blue)
            );

            var admin = ApplicationDbContextSeedData.GetSeedUsers().First();
            var tempMessage = $"Administrator User: Email: {admin.Email}, Password: Admin123";
            AnsiConsole.MarkupLine($"[bold]{tempMessage}[/]");
            
            var email = AnsiConsole.Prompt(
                new TextPrompt<string>("What's your email?"));
            var password = AnsiConsole.Prompt(
                new TextPrompt<string>("What's your password?")
                    .Secret());
            
            await AnsiConsole.Status()
                .StartAsync("Loading...", async ctx => 
                {
                    var loginCommand = new LoginUserCommand(email, password);
                    loginResult =  await mediator.Send(loginCommand, token);
                });
            
            //handle the try again
            tryAgain = loginResult?.Match(
                onSuccess: () =>
                {
                    AnsiConsole.MarkupLine("[green]Login successful[/]");
                    return false;
                },
                onFailure: error =>
                {
                    var friendlyError = GetErrorMessage(error);
                    AnsiConsole.MarkupLine($"[red]{friendlyError}[/]");
                    return AnsiConsole.Confirm("Do you want to try again?");
                }) ?? false;
            
        }
        
        //handle the result
        return loginResult?.Match(
            onSuccess: token => token,
            onFailure: _ => string.Empty);
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



/// <summary>/// Spectre Console Menu Screen with options to Login, Register or Exit/// </summary>
public class MenuScreen : IScreen<MenuSelection>
{
    private static readonly Dictionary<string, MenuSelection> MenuOptions = new()
    {
        ["Dashboard"] = MenuSelection.Dashboard,
        ["Elevator Control"] = MenuSelection.ElevatorControl,
        ["Multi Request Elevator Control"] = MenuSelection.MultiElevatorControl,
        ["Login"] = MenuSelection.Login,
        ["Exit"] = MenuSelection.Exit
    };

    public async Task<Result<MenuSelection>> ShowAsync(CancellationToken token)
    {
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Welcome to the Elevator Management Simulator")
                .PageSize(10)
                .AddChoices(MenuOptions.Keys));

        if (MenuOptions.TryGetValue(selection, out var result))
        {
            AnsiConsole.MarkupLine($"[bold]{selection}[/]");
            return await Task.FromResult(Result.Success(result));
        }

        return Result.Failure<MenuSelection>(
            Error.Failure("Menu.InvalidSelection", "An invalid menu option was selected."));
    }
}



public class ConfigurationMenu(IServiceProvider serviceProvider) : IScreen<bool>
{
    //Dictionary to hold the configuration menu options
    private static readonly Dictionary<string, ConfigurationMenuSelection> ConfigurationMenuOptions = new()
    {
        ["Register a user"] = ConfigurationMenuSelection.Register,
        ["Exit"] = ConfigurationMenuSelection.Exit
    };
    
    public async Task<Result<bool>> ShowAsync(CancellationToken token)
    {
        //Prompt the user to select a configuration menu option
        var currentSelection = ConfigurationMenuSelection.None;
        while (currentSelection != ConfigurationMenuSelection.Exit)
        {
            AnsiConsole.Clear();
            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Configuration Menu")
                    .PageSize(10)
                    .AddChoices(ConfigurationMenuOptions.Keys));

            //If the selected option is valid, return the corresponding ConfigurationMenuSelection
            if (ConfigurationMenuOptions.TryGetValue(selection, out var result))
            {
                AnsiConsole.MarkupLine($"[bold]{selection}[/]");
                currentSelection = result;
                switch (currentSelection)
                {
                    case ConfigurationMenuSelection.Register:
                        var registerScreen = serviceProvider.GetRequiredService<RegisterScreen>();
                        await registerScreen.ShowAsync(token);
                        break;
                    case ConfigurationMenuSelection.Exit:
                        return Result.Success(true);
                }
            }
        }
        return Result.Success(true);
    }
}




public class RegisterScreen(IMediator mediator) : IScreen<bool>
{
    public async Task<Result<bool>> ShowAsync(CancellationToken token)
    {
        Result<Guid>? registrationResult = null;
        var tryAgain = true;
        
        while (tryAgain)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[bold]User Register[/]");
            var name = AnsiConsole.Prompt(
                new TextPrompt<string>("What's your name?"));
            var surname = AnsiConsole.Prompt(
                new TextPrompt<string>("What's your surname?"));
            var email = AnsiConsole.Prompt(
                new TextPrompt<string>("What's your email?"));
            var password = AnsiConsole.Prompt(
                new TextPrompt<string>("What's your password?")
                    .Secret());
            
            await AnsiConsole.Status()
                .StartAsync("Thinking...", async ctx => 
                {
                    var registerNewUserCommand = new RegisterUserCommand(email, name, surname, password);
                    registrationResult =  await mediator.Send(registerNewUserCommand, token);
                   
                });
            
            tryAgain = registrationResult?.Match(
                onSuccess: () =>
                {
                    AnsiConsole.MarkupLine("[green]Registration successful[/]");
                    return false;
                },
                onFailure: error =>
                {
                    var friendlyError = GetErrorMessage(error);
                    AnsiConsole.MarkupLine($"[red]{friendlyError}[/]");
                    return AnsiConsole.Confirm("Do you want to try again?");
                }) ?? false;
            
        }
        return registrationResult?.IsSuccess ?? false;
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
        
        
        var buildingId = ApplicationDbContextSeedData.GetSeedBuildings().First()!.Id;
        Result<IEnumerable<ElevatorItem>> result = null;
        var stopWatch = new Stopwatch();
        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                AnsiConsole.Write(
                    new FigletText(" Dashboard")
                        .LeftJustified()
                        .Color(Color.Blue)
                );

                stopWatch.Restart();
                await AnsiConsole.Status()
                    .StartAsync("Retrieving configured elevators...", async ctx =>
                    {
                        var elevatorPoolService = serviceProvider.GetRequiredService<IInMemoryElevatorPoolService>();
                        result =
                            await elevatorPoolService.GetAllElevatorsAsync(buildingId, token);
                    });
                stopWatch.Stop();

                _ = result?.Match(
                    onSuccess: elevators =>
                    {
                        var table = new Table();
                        table.AddColumn("Elevator Number");
                        table.AddColumn("Current Floor");
                        table.AddColumn("Destination Floor");
                        table.AddColumn("Queue");
                        table.AddColumn("Direction");
                        table.AddColumn("Status");
                        table.AddColumn("Door Status");
                        table.AddColumn("Type");
                        table.AddColumn("Speed");
                        table.AddColumn("Capacity");
                        foreach (var elevator in elevators.OrderBy(a => a.Number))
                        {
                            table.AddRow(
                                elevator.Number.ToString(),
                                elevator.CurrentFloor.ToString(),
                                elevator.DestinationFloor.ToString(),
                                elevator.DestinationFloors.Count.ToString(),
                                elevator.ElevatorDirection.ToString(),
                                elevator.ElevatorStatus == ElevatorStatus.Active
                                    ? $"[green]{elevator.ElevatorStatus.ToString()}[/]"
                                    : $"[red]{elevator.ElevatorStatus.ToString()}[/]",
                                elevator.DoorStatus.ToString(),
                                elevator.ElevatorType.ToString(),
                                elevator.FloorsPerSecond.ToString(),
                                elevator.QueueCapacity.ToString());
                        }

                        AnsiConsole.Write(table);
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

                AnsiConsole.WriteLine("Refresh took: " + stopWatch.ElapsedMilliseconds + "ms");
                AnsiConsole.MarkupLine("Press [green]C[/] to exit ...");
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


/// <summary> Elevator Control Screen to request an elevator to a specific floor. </summary>
public class ElevatorControlScreen(IServiceProvider serviceProvider): IScreen<bool>
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

            var floor = AnsiConsole.Prompt(
                new TextPrompt<int>("What floor are you on?"));

            //DoTo: Direction prompt
            var buildingId = ApplicationDbContextSeedData.GetSeedBuildings().First()!.Id;
            await AnsiConsole.Status()
                .StartAsync("Requesting elevator...", async ctx =>
                {
                    var scope = serviceProvider.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    var request = new RequestElevatorCommand(buildingId, floor);
                    result = await mediator.Send(request, token);
                });

            anotherRequest = result?.Match(
                onSuccess: () =>
                {
                    AnsiConsole.MarkupLine("[green]Elevator requested successfully[/]");
                    return AnsiConsole.Confirm("Do you want to request another elevator?");
                },
                onFailure: error =>
                {
                    var friendlyError = GetErrorMessage(error);
                    AnsiConsole.MarkupLine($"[red]{friendlyError}[/]");
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

