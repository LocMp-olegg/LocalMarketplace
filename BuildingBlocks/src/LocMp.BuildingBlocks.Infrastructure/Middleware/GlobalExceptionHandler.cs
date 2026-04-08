using System.Security.Authentication;
using FluentValidation;
using LocMp.BuildingBlocks.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LocMp.BuildingBlocks.Infrastructure.Middleware;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService,
    IHostEnvironment env) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken ct)
    {
        var (statusCode, title) = MapException(exception);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Instance = httpContext.Request.Path,
            Detail = env.IsDevelopment() ? exception.ToString() : exception.Message,
            Extensions =
            {
                ["traceId"] = httpContext.TraceIdentifier
            }
        };

        if (exception is AppException appEx)
            problemDetails.Extensions["errorCode"] = appEx.ErrorCode;

        if (exception is ValidationException valEx)
        {
            problemDetails.Extensions["errors"] = valEx.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());
        }

        LogException(exception, statusCode);

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });
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