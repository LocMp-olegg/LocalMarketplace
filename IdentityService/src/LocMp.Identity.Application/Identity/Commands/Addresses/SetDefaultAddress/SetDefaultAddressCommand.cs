using MediatR;

namespace LocMp.Identity.Application.Identity.Commands.Addresses.SetDefaultAddress;

public sealed record SetDefaultAddressCommand(Guid UserId, Guid AddressId) : IRequest;
