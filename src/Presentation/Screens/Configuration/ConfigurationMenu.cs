using Application.Abstractions.Screen;
using Application.Screens;
using Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Presentation.Screens;

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
