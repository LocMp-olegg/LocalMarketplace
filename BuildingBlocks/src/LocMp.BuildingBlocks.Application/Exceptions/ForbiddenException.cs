using Microsoft.AspNetCore.Http;

namespace LocMp.BuildingBlocks.Application.Exceptions;

public class ForbiddenException(string message = "Forbidden")
    : AppException(message, StatusCodes.Status403Forbidden, "Forbidden");