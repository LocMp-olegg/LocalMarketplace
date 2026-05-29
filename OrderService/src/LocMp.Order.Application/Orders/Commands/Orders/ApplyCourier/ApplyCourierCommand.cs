using LocMp.Order.Application.DTOs;
using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Orders.ApplyCourier;

public sealed record ApplyCourierCommand(
    Guid CourierId,
    Guid OrderId,
    string CourierName,
    string CourierPhone,
    double? Latitude,
    double? Longitude) : IRequest<CourierApplicationDto>;