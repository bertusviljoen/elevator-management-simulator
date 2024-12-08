using Domain.Common;

namespace Application.Abstractions.Screen;

/// <summary>
/// Represents a screen in the application.
/// </summary>
/// <typeparam name="TResult">The type of result this screen produces.</typeparam>
public interface IScreen<TResult>
{
    /// <summary>
    /// Gets the supported result type for this screen.
    /// </summary>
    Type ResultType => typeof(TResult);

    /// <summary>
    /// Shows the screen and handles user interaction.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A result containing the screen's output.</returns>
    Task<Result<TResult>> ShowAsync(CancellationToken token);
}
