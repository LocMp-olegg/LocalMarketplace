using LocMp.Identity.Application.DTOs.Courier;
using MediatR;

namespace LocMp.Identity.Application.Identity.Commands.Courier.UpdateCourierProfile;

public sealed record UpdateCourierProfileCommand(
    Guid UserId,
    bool IsActive,
    int ServiceRadiusMeters,
    double? Latitude,
    double? Longitude) : IRequest<CourierProfileDto>;