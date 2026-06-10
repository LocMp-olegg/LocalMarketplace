using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Identity.Application.DTOs.Courier;
using LocMp.Identity.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace LocMp.Identity.Application.Identity.Commands.Courier.UpdateCourierProfile;

public sealed class UpdateCourierProfileCommandHandler(
    ApplicationDbContext db,
    IMapper mapper
) : IRequestHandler<UpdateCourierProfileCommand, CourierProfileDto>
{
    public async Task<CourierProfileDto> Handle(UpdateCourierProfileCommand request, CancellationToken ct)
    {
        var profile = await db.CourierProfiles
                          .FirstOrDefaultAsync(x => x.CourierId == request.UserId, ct)
                      ?? throw new NotFoundException($"Courier profile for user '{request.UserId}' not found.");

        profile.IsActive = request.IsActive;
        profile.ServiceRadiusMeters = request.ServiceRadiusMeters;
        profile.BaseLocation = request is { Latitude: not null, Longitude: not null }
            ? new Point(request.Longitude.Value, request.Latitude.Value) { SRID = 4326 }
            : null;
        profile.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        return mapper.Map<CourierProfileDto>(profile);
    }
}