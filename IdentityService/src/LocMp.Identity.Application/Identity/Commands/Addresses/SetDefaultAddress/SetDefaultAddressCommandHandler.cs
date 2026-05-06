using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Identity.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Identity.Application.Identity.Commands.Addresses.SetDefaultAddress;

public sealed class SetDefaultAddressCommandHandler(ApplicationDbContext db)
    : IRequestHandler<SetDefaultAddressCommand>
{
    public async Task Handle(SetDefaultAddressCommand request, CancellationToken ct)
    {
        var exists = await db.UserAddresses
            .AnyAsync(a => a.Id == request.AddressId && a.UserId == request.UserId, ct);

        if (!exists)
            throw new NotFoundException($"Address '{request.AddressId}' not found.");

        await db.UserAddresses
            .Where(a => a.UserId == request.UserId && a.IsDefault)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false), ct);

        await db.UserAddresses
            .Where(a => a.Id == request.AddressId)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, true), ct);
    }
}
