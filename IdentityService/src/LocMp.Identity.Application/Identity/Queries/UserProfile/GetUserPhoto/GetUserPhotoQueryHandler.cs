using LocMp.Identity.Application.DTOs.UserProfile;
using LocMp.Identity.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Identity.Application.Identity.Queries.UserProfile.GetUserPhoto;

public sealed class GetUserPhotoQueryHandler(ApplicationDbContext dbContext)
    : IRequestHandler<GetUserPhotoQuery, UserPhotoDto>
{
    public async Task<UserPhotoDto> Handle(GetUserPhotoQuery request, CancellationToken ct)
    {
        var photoDto = await dbContext.UserPhotos
            .AsNoTracking()
            .Where(p => p.UserId == request.UserId)
            .Select(p => new UserPhotoDto(
                p.PhotoData,
                p.MimeType,
                p.UploadedAt
            ))
            .FirstOrDefaultAsync(ct);

        return photoDto ?? throw new KeyNotFoundException($"Photo for user '{request.UserId}' not found.");
    }
}