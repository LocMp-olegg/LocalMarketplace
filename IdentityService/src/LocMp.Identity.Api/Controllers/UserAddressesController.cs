using LocMp.BuildingBlocks.Infrastructure.Extensions;
using LocMp.Identity.Api.Requests.Addresses;
using LocMp.Identity.Application.DTOs.UserAddress;
using LocMp.Identity.Application.Identity.Commands.Addresses.CreateUserAddress;
using LocMp.Identity.Application.Identity.Commands.Addresses.DeleteUserAddress;
using LocMp.Identity.Application.Identity.Commands.Addresses.SetDefaultAddress;
using LocMp.Identity.Application.Identity.Commands.Addresses.UpdateUserAddress;
using LocMp.Identity.Application.Identity.Queries.Addresses.GetUserAddressById;
using LocMp.Identity.Application.Identity.Queries.Addresses.GetUserAddresses;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocMp.Identity.Api.Controllers;

[ApiController]
[Route("api/profile/addresses")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class UserAddressesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserAddressDto>>> GetAll(CancellationToken ct)
    {
        var result = await sender.Send(new GetUserAddressesQuery(HttpContext.GetUserId()), ct);
        return Ok(result);
    }

    [HttpGet("{addressId:guid}")]
    public async Task<ActionResult<UserAddressDto>> GetById(Guid addressId, CancellationToken ct)
    {
        var result = await sender.Send(new GetUserAddressByIdQuery(HttpContext.GetUserId(), addressId), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<UserAddressDto>> Create([FromBody] CreateUserAddressRequest request,
        CancellationToken ct)
    {
        var command = new CreateUserAddressCommand(
            UserId: HttpContext.GetUserId(),
            Title: request.Title,
            City: request.City,
            Street: request.Street,
            HouseNumber: request.HouseNumber,
            Apartment: request.Apartment,
            Entrance: request.Entrance,
            Floor: request.Floor,
            Latitude: request.Latitude,
            Longitude: request.Longitude,
            IsDefault: request.IsDefault
        );

        var result = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { addressId = result.Id }, result);
    }

    [HttpPut("{addressId:guid}")]
    public async Task<ActionResult<UserAddressDto>> Update(Guid addressId,
        [FromBody] UpdateUserAddressRequest request, CancellationToken ct)
    {
        var command = new UpdateUserAddressCommand(
            UserId: HttpContext.GetUserId(),
            AddressId: addressId,
            Title: request.Title,
            City: request.City,
            Street: request.Street,
            HouseNumber: request.HouseNumber,
            Apartment: request.Apartment,
            Entrance: request.Entrance,
            Floor: request.Floor,
            Latitude: request.Latitude,
            Longitude: request.Longitude
        );

        var result = await sender.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("{addressId:guid}")]
    public async Task<IActionResult> Delete(Guid addressId, CancellationToken ct)
    {
        await sender.Send(new DeleteUserAddressCommand(HttpContext.GetUserId(), addressId), ct);
        return NoContent();
    }

    [HttpPost("{addressId:guid}/set-default")]
    public async Task<IActionResult> SetDefault(Guid addressId, CancellationToken ct)
    {
        await sender.Send(new SetDefaultAddressCommand(HttpContext.GetUserId(), addressId), ct);
        return NoContent();
    }
}
