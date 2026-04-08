using Microsoft.AspNetCore.Http;

namespace LocMp.BuildingBlocks.Application.Exceptions;

public class NotFoundException(string message) : AppException(message, StatusCodes.Status404NotFound, "Not Found");