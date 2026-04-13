using LocMp.Order.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LocMp.Order.Application.Orders.Commands.Disputes.UploadDisputePhotos;

public sealed record UploadDisputePhotosCommand(
    Guid DisputeId,
    Guid UploadedById,
    bool IsAdmin,
    IReadOnlyList<IFormFile> Images) : IRequest<IReadOnlyList<DisputePhotoDto>>;