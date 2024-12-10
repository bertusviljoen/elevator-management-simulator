using System.Reflection;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Domain.Common;
using Microsoft.Extensions.Logging;

namespace Application.Abstractions.Behaviors;

internal sealed class ValidationPipelineBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<ValidationPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Validating request {Request}", request);
        ValidationFailure[] validationFailures = await ValidateAsync(request);

        logger.LogInformation("Validation failures Count: {ValidationFailures}", validationFailures.Length);
        if (validationFailures.Length == 0)
        {
            return await next();
        }

        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            logger.LogInformation("Returning validation failure result");
            Type resultType = typeof(TResponse).GetGenericArguments()[0];
            logger.LogInformation("Result type: {ResultType}", resultType);

            MethodInfo? failureMethod = typeof(Result<>)
                .MakeGenericType(resultType)
                .GetMethod(nameof(Result<object>.ValidationFailure));

            logger.LogInformation("Failure method: {FailureMethod}", failureMethod);
            if (failureMethod is not null)
            {
                logger.LogInformation("Invoking failure method");
                return (TResponse)failureMethod.Invoke(
                    null,
                    [CreateValidationError(validationFailures)]);
            }
        }
        else if (typeof(TResponse) == typeof(Result))
        {
            logger.LogInformation("Returning validation failure result");
            return (TResponse)(object)Result.Failure(CreateValidationError(validationFailures));
        }

        logger.LogInformation("Throwing validation exception");
        throw new ValidationException(validationFailures);
    }

    private async Task<ValidationFailure[]> ValidateAsync(TRequest request)
    {
        logger.LogInformation("Validating request {Request}", request);
        logger.LogInformation("Validators Count: {ValidatorsCount}", validators.Count());
        if (!validators.Any())
        {
            return [];
        }

        logger.LogInformation("Validating request {Request}", request);
        var context = new ValidationContext<TRequest>(request);

        
        ValidationResult[] validationResults = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(context)));
        
        logger.LogInformation("Validation results Count: {ValidationResultsCount}", validationResults.Length);

        ValidationFailure[] validationFailures = validationResults
            .Where(validationResult => !validationResult.IsValid)
            .SelectMany(validationResult => validationResult.Errors)
            .ToArray();

        logger.LogInformation("Validation failures Count: {ValidationFailuresCount}", validationFailures.Length);
        return validationFailures;
    }

    private static ValidationError CreateValidationError(ValidationFailure[] validationFailures) =>
        new(validationFailures.Select(f => Error.Problem(f.ErrorCode, f.ErrorMessage)).ToArray());
}
