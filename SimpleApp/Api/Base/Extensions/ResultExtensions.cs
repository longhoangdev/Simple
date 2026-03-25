using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace SimpleApp.Api.Base.Extensions;

/// <summary>
/// Provides extension methods for converting FluentResults into HTTP results.
/// </summary>
internal static class ResultExtensions
{
    /// <summary>
    /// Converts a FluentResults Result into an HTTP result, encapsulating the success value or any errors as ProblemDetails.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the successful result.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>An HTTP result representing either the success value or a ProblemDetails object with errors.</returns>
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        return result.IsSuccess ? Results.Ok(result.Value) : CreateProblemDetailsFromErrors(result.Errors);
    }

    /// <summary>
    /// Converts a FluentResults Result (non-generic) into an HTTP result, encapsulating any errors as ProblemDetails.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <returns>An HTTP result representing either a success or a ProblemDetails object with errors.</returns>
    public static IResult ToHttpResult(this Result result)
    {
        if (result.IsSuccess)
            return Results.Ok();

        return CreateProblemDetailsFromErrors(result.Errors);
    }

    /// <summary>
    /// Creates a ProblemDetails object from a collection of FluentResults errors, categorizing them into validation and generic errors.
    /// </summary>
    /// <param name="errors">The collection of errors to process.</param>
    /// <returns>A ProblemDetails object containing detailed information about the errors.</returns>
    private static IResult CreateProblemDetailsFromErrors(ICollection<IError> errors)
    {
        var problemDetails = new ProblemDetails
        {
            Title = "An error occurred",
            Status = 422,
            Type = "https://httpstatuses.com/422",
        };

        var validationErrors = errors
            .OfType<ValidationError>()
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.SelectMany(e => e.Reasons.Select(r => r.Message)).Distinct().ToArray()
            );

        var nonValidationErrors = errors.Where(e => e is not ValidationError);
        var enumerable = nonValidationErrors as IError[] ?? nonValidationErrors.ToArray();
        var genericErrors = enumerable
            .SelectMany(e => e.Reasons.Select(r => r.Message))
            .Distinct()
            .ToList();

        if (!genericErrors.Any() && enumerable.Length != 0)
            genericErrors = enumerable.Select(x => x.Message).Distinct().ToList();

        if (validationErrors.Any())
            problemDetails.Extensions.Add("validationErrors", validationErrors);

        if (genericErrors.Any())
            problemDetails.Extensions.Add("genericErrors", genericErrors);

        return Results.Problem(problemDetails);
    }
}