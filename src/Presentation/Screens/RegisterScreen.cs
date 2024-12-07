using Application.Abstractions.Screen;
using Application.Users.Register;
using MediatR;
using Presentation.Extensions;
using SharedKernel;
using Spectre.Console;

namespace Presentation.Screens;

public class RegisterScreen(IMediator mediator) : IScreen<bool>
{
    public async Task<Result<bool>> ShowAsync(CancellationToken token)
    {
        var correct = false;
        var tryAgain = true;
        while (tryAgain)
        {
            var name = AnsiConsole.Prompt(
                new TextPrompt<string>("What's your name?"));
            var surname = AnsiConsole.Prompt(
                new TextPrompt<string>("What's your surname?"));
            var email = AnsiConsole.Prompt(
                new TextPrompt<string>("What's your email?"));
            var password = AnsiConsole.Prompt(
                new TextPrompt<string>("What's your password?")
                    .Secret());
            
            // Asynchronous
            await AnsiConsole.Status()
                .StartAsync("Thinking...", async ctx => 
                {
                    var registerNewUserCommand = new RegisterUserCommand(name, surname, email, password);
                    var result =  await mediator.Send(registerNewUserCommand, token);
                    correct = result.Match(
                        success => tryAgain = false,
                        failure =>
                        {
                            var resultError = result.Error;
                            AnsiConsole.MarkupLine($"[red]Error registering user[/]");
                            
                            return AnsiConsole.Confirm("Do you want to try again?");
                        });
                
                });
            
            
        }
        return correct;
    }
}
