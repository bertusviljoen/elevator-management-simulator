using Application.Abstractions.Screen;
using Application.Users.Login;
using MediatR;
using Presentation.Extensions;
using SharedKernel;
using Spectre.Console;

namespace Presentation.Screens;

public class LoginScreen(IMediator mediator) : IScreen<string>
{
    public async Task<Result<string>> ShowAsync(CancellationToken token)
    {
        Result<string>? loginResult = null;
        var tryAgain = true;
        
        while (tryAgain)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[bold]User Login[/]");
            var email = AnsiConsole.Prompt(
                new TextPrompt<string>("What's your email?"));
            var password = AnsiConsole.Prompt(
                new TextPrompt<string>("What's your password?")
                    .Secret());
            
            await AnsiConsole.Status()
                .StartAsync("Thinking...", async ctx => 
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
