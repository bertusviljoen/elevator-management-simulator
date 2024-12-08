using Application.Abstractions.Screen;
using Application.Users.Register;
using MediatR;
using Microsoft.AspNetCore.Http;
using Presentation.Extensions;
using SharedKernel;
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
                    var registerNewUserCommand = new RegisterUserCommand(name, surname, email, password);
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
        
        if (error.Error is not ValidationError validationError)
        {
            return "null";
        }

        return string.Join(", ", validationError.Errors.Select(e => e.Description));
        
        
         static string GetTitle(Error error) =>
            error.Type switch
            {
                ErrorType.Validation => error.Code,
                ErrorType.Problem => error.Code,
                ErrorType.NotFound => error.Code,
                ErrorType.Conflict => error.Code,
                _ => "Server failure"
            };

        static string GetDetail(Error error) =>
            error.Type switch
            {
                ErrorType.Validation => error.Description,
                ErrorType.Problem => error.Description,
                ErrorType.NotFound => error.Description,
                ErrorType.Conflict => error.Description,
                _ => "An unexpected error occurred"
            };

        static string GetType(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                ErrorType.Problem => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            };

        static int GetStatusCode(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };
        
    }
}
