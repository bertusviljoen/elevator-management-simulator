using Domain.Common;

namespace Presentation.Extensions;

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
