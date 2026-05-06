using MediatR;

namespace LocMp.Identity.Application.Identity.Commands.Addresses.DeleteUserAddress;

public sealed record DeleteUserAddressCommand(Guid UserId, Guid AddressId) : IRequest;
