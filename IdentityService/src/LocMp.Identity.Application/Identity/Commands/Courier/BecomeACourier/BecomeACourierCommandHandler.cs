using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Identity;
using LocMp.Identity.Application.DTOs.Courier;
using LocMp.Identity.Domain.Entities;
using LocMp.Identity.Domain.Enums;
using LocMp.Identity.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Identity.Application.Identity.Commands.Courier.BecomeACourier;

public sealed class BecomeACourierCommandHandler(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext db,
    IEventBus eventBus,
    IMapper mapper
) : IRequestHandler<BecomeACourierCommand, CourierProfileDto>
{
    public async Task<CourierProfileDto> Handle(BecomeACourierCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString()).ConfigureAwait(false)
                   ?? throw new NotFoundException($"User '{request.UserId}' not found.");

        var alreadyExists = await db.CourierProfiles
            .AnyAsync(x => x.CourierId == request.UserId, ct);

        if (alreadyExists)
            throw new ConflictException("Courier profile already exists.");

        var roleResult = await userManager.AddToRoleAsync(user, nameof(UserRole.Courier)).ConfigureAwait(false);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to assign Courier role: {errors}");
        }

        var profile = new CourierProfile(request.UserId);
        db.CourierProfiles.Add(profile);
        await db.SaveChangesAsync(ct);

        var displayName = $"{user.FirstName} {user.LastName}".Trim();
        await eventBus.PublishAsync(
            new UserBecameCourierEvent(user.Id, displayName, DateTimeOffset.UtcNow), ct);

        return mapper.Map<CourierProfileDto>(profile);
    }
}