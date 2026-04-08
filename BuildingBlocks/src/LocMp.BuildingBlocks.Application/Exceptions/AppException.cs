using Microsoft.AspNetCore.Http;

namespace LocMp.BuildingBlocks.Application.Exceptions;

public abstract class AppException(
    string message,
    int statusCode = StatusCodes.Status400BadRequest,
    string? title = null) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string Title { get; } = title ?? "Application Error";

    public string ErrorCode => GetType().Name.Replace("Exception", string.Empty);
}