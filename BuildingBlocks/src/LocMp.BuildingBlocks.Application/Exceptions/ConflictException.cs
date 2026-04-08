using Microsoft.AspNetCore.Http;

namespace LocMp.BuildingBlocks.Application.Exceptions;

public class ConflictException(string message) : AppException(message, StatusCodes.Status409Conflict, "Conflict");