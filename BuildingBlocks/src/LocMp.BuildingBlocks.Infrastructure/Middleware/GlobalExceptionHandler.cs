using System.Security.Authentication;
using FluentValidation;
using LocMp.BuildingBlocks.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LocMp.BuildingBlocks.Infrastructure.Middleware;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    ProblemDetailsFactory problemDetailsFactory,
    IHostEnvironment env) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken ct)
    {
        var (statusCode, title) = MapException(exception);

        var problemDetails = problemDetailsFactory.CreateProblemDetails(
            httpContext,
            statusCode: statusCode,
            title: title,
            instance: httpContext.Request.Path
        );

        problemDetails.Extensions["traceId"] = System.Diagnostics.Activity.Current?.Id
                                               ?? httpContext.TraceIdentifier;
        if (exception is ValidationException valEx)
        {
            problemDetails.Detail = "One or more validation errors occurred.";
            problemDetails.Extensions["errors"] = valEx.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());
        }
        else if (exception is AppException appEx)
        {
            problemDetails.Detail = appEx.Message;
            problemDetails.Extensions["errorCode"] = appEx.ErrorCode;
        }
        else
        {
            problemDetails.Detail = env.IsDevelopment() ? exception.ToString() : "An unexpected error occurred.";
        }

        LogException(exception, statusCode);

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: ct);

        return true;
    }

    private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
    {
        AppException appEx => (appEx.StatusCode, appEx.Title),
        ArgumentNullException => (StatusCodes.Status400BadRequest, "Invalid argument provided"),
        ValidationException => (StatusCodes.Status400BadRequest, "Validation Error"),
        UnauthorizedAccessException or AuthenticationException => (StatusCodes.Status401Unauthorized,
            "Unauthorized access"),
        TimeoutException or TaskCanceledException
            => (StatusCodes.Status408RequestTimeout, "Request timed out"),
        _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
    };

    private void LogException(Exception ex, int statusCode)
    {
        if (statusCode >= 500)
            logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
        else
            logger.LogWarning("Handled application exception: {Message}", ex.Message);
    }
}