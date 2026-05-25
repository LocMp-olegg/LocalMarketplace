using LocMp.Identity.Application.DTOs.Courier;
using MediatR;

namespace LocMp.Identity.Application.Identity.Queries.Courier.GetCourierProfile;

public sealed record GetCourierProfileQuery(Guid UserId) : IRequest<CourierProfileDto>;