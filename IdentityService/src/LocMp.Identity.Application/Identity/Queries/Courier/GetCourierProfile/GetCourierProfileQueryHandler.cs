using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Identity.Application.DTOs.Courier;
using LocMp.Identity.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Identity.Application.Identity.Queries.Courier.GetCourierProfile;

public sealed class GetCourierProfileQueryHandler(
    ApplicationDbContext db,
    IMapper mapper
) : IRequestHandler<GetCourierProfileQuery, CourierProfileDto>
{
    public async Task<CourierProfileDto> Handle(GetCourierProfileQuery request, CancellationToken ct)
    {
        var profile = await db.CourierProfiles
                          .AsNoTracking()
                          .FirstOrDefaultAsync(x => x.CourierId == request.UserId, ct)
                      ?? throw new NotFoundException($"Courier profile for user '{request.UserId}' not found.");

        return mapper.Map<CourierProfileDto>(profile);
    }
}