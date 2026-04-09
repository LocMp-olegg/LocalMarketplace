using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Identity;
using LocMp.Identity.Application.DTOs.User;
using LocMp.Identity.Domain.Entities;
using LocMp.Identity.Domain.Enums;
using LocMp.Identity.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;

namespace LocMp.Identity.Application.Identity.Commands.Users.RegisterUser;

public sealed class RegisterUserCommandHandler(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext db,
    IEventBus eventBus,
    IMapper mapper
) : IRequestHandler<RegisterUserCommand, UserDto>
{
    public async Task<UserDto> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        const string defaultRole = nameof(UserRole.User);

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Gender = (int?)request.Gender,
            BirthDate = request.BirthDate,
            Active = true,
            RegisteredAt = DateTimeOffset.UtcNow,
            EmailConfirmed = false
        };

        var result = await userManager.CreateAsync(user, request.Password).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ConflictException($"Failed to register user '{request.Email}': {errors}");
        }

        var roleResult = await userManager.AddToRoleAsync(user, defaultRole).ConfigureAwait(false);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException(
                $"User created but role '{defaultRole}' assignment failed: {errors}");
        }

        if (request.IsSeller)
            await userManager.AddToRoleAsync(user, nameof(UserRole.Seller));

        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            db.UserAddresses.Add(new UserAddress
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Title = "Основной",
                Location = new Point(request.Longitude.Value, request.Latitude.Value) { SRID = 4326 },
                IsDefault = true,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync(ct);
        }

        await eventBus.PublishAsync(
            new UserRegisteredEvent(user.Id, user.Email!, $"{user.FirstName} {user.LastName}".Trim(),
                user.RegisteredAt), ct);

        if (request.IsSeller)
        {
            var displayName = $"{user.FirstName} {user.LastName}".Trim();
            await eventBus.PublishAsync(
                new UserBecameSellerEvent(user.Id, displayName, DateTimeOffset.UtcNow), ct);
        }

        var roles = await userManager.GetRolesAsync(user);
        return mapper.Map<UserDto>(user) with { Roles = [.. roles] };
    }
}