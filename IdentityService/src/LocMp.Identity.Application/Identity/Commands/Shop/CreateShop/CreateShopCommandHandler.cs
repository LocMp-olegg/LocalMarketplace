using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Identity;
using LocMp.Identity.Application.DTOs.Shop;
using LocMp.Identity.Domain.Entities;
using LocMp.Identity.Domain.Enums;
using LocMp.Identity.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;

namespace LocMp.Identity.Application.Identity.Commands.Shop.CreateShop;

public sealed class CreateShopCommandHandler(
    ApplicationDbContext db,
    UserManager<ApplicationUser> userManager,
    IEventBus eventBus)
    : IRequestHandler<CreateShopCommand, ShopProfileDto>
{
    public async Task<ShopProfileDto> Handle(CreateShopCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString())
                   ?? throw new NotFoundException($"User '{request.UserId}' not found.");

        Point? location = null;
        if (request.Latitude.HasValue && request.Longitude.HasValue)
            location = new Point(request.Longitude.Value, request.Latitude.Value) { SRID = 4326 };

        var shop = new ShopProfile
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            BusinessName = request.BusinessName,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            Description = request.Description,
            Inn = request.Inn,
            BusinessType = request.BusinessType,
            WorkingHours = request.WorkingHours,
            ServiceRadiusMeters = request.ServiceRadiusMeters,
            Location = location,
            IsVerified = true,
            VerifiedAt = DateTimeOffset.UtcNow,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.ShopProfiles.Add(shop);

        // Выдаём роль Seller только если её ещё не было
        var isAlreadySeller = await userManager.IsInRoleAsync(user, nameof(UserRole.Seller));
        if (!isAlreadySeller)
        {
            await userManager.AddToRoleAsync(user, nameof(UserRole.Seller));
            var displayName = $"{user.FirstName} {user.LastName}".Trim();
            await eventBus.PublishAsync(
                new UserBecameSellerEvent(user.Id, displayName, DateTimeOffset.UtcNow), ct);
        }

        await db.SaveChangesAsync(ct);

        var sellerDisplayName = $"{user.FirstName} {user.LastName}".Trim();
        await eventBus.PublishAsync(
            new ShopProfileCreatedEvent(shop.Id, user.Id, shop.BusinessName, sellerDisplayName, DateTimeOffset.UtcNow), ct);

        return ToDto(shop);
    }

    internal static ShopProfileDto ToDto(ShopProfile s) => new(
        s.Id,
        s.UserId,
        s.BusinessName,
        s.PhoneNumber,
        s.Email,
        s.Description,
        s.BusinessType,
        s.WorkingHours,
        s.ServiceRadiusMeters,
        s.Location?.Y,
        s.Location?.X,
        s.AvatarUrl,
        s.IsVerified,
        s.IsActive,
        s.CreatedAt,
        s.UpdatedAt);
}
