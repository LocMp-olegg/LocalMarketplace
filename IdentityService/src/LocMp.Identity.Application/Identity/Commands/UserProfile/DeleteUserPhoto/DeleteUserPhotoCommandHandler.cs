using LocMp.Identity.Domain.Entities;
using LocMp.Identity.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Identity.Application.Identity.Commands.UserProfile.DeleteUserPhoto;

public sealed class DeleteUserPhotoCommandHandler(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext
) : IRequestHandler<DeleteUserPhotoCommand>
{
    public async Task Handle(DeleteUserPhotoCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.Users
            .Include(u => u.Photo)
            .FirstOrDefaultAsync(u => u.Id == (request.UserId), cancellationToken);

        if (user is null)
            throw new KeyNotFoundException($"User with id '{request.UserId}' was not found");

        if (user.Photo is null)
            throw new InvalidOperationException("User does not have a photo");

        dbContext.UserPhotos.Remove(user.Photo);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}