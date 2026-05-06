using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Identity.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Identity.Application.Identity.Commands.Addresses.DeleteUserAddress;

public sealed class DeleteUserAddressCommandHandler(ApplicationDbContext db)
    : IRequestHandler<DeleteUserAddressCommand>
{
    public async Task Handle(DeleteUserAddressCommand request, CancellationToken ct)
    {
        var address = await db.UserAddresses
                          .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == request.UserId, ct)
                      ?? throw new NotFoundException($"Address '{request.AddressId}' not found.");

        db.UserAddresses.Remove(address);
        await db.SaveChangesAsync(ct);
    }
}
