using Application.Abstractions.Screen;
using Application.Users.Register;
using MediatR;
using Microsoft.AspNetCore.Http;
using Presentation.Extensions;
using Domain.Common;
using Spectre.Console;

namespace Presentation.Screens;

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
