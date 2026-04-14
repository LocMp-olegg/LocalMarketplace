using LocMp.Review.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LocMp.Review.Application.Reviews.Commands.UploadReviewPhotos;

public sealed record UploadReviewPhotosCommand(
    Guid ReviewId,
    Guid UploadedById,
    bool IsAdmin,
    IReadOnlyList<IFormFile> Images) : IRequest<IReadOnlyList<ReviewPhotoDto>>;