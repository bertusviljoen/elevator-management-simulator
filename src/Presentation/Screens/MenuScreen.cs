using Spectre.Console;
using Application.Abstractions.Screen;
using Application.Screens;
using SharedKernel;

namespace Presentation.Screens;

/// <summary>/// Spectre Console Menu Screen with options to Login, Register or Exit/// </summary>
public class MenuScreen : IScreen<MenuSelection>
{
    private static readonly Dictionary<string, MenuSelection> MenuOptions = new()
    {
        ["Login"] = MenuSelection.Login,
        ["Register"] = MenuSelection.Register,
        ["Exit"] = MenuSelection.Exit
    };

    public async Task<Result<MenuSelection>> ShowAsync(CancellationToken token)
    {
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Welcome to the Elevator Management Simulator")
                .PageSize(3)
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
