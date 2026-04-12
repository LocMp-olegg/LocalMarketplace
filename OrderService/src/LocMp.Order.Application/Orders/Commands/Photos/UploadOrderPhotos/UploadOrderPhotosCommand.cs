using LocMp.Order.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LocMp.Order.Application.Orders.Commands.Photos.UploadOrderPhotos;

public sealed record UploadOrderPhotosCommand(
    Guid OrderId,
    Guid UploadedById,
    bool IsAdmin,
    IReadOnlyList<IFormFile> Images) : IRequest<IReadOnlyList<OrderPhotoDto>>;
