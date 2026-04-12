using LocMp.Order.Api.Extensions;
using LocMp.Order.Api.Requests;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Application.Orders.Commands.Cart.AddToCart;
using LocMp.Order.Application.Orders.Commands.Cart.ClearCart;
using LocMp.Order.Application.Orders.Commands.Cart.RemoveFromCart;
using LocMp.Order.Application.Orders.Commands.Orders.Checkout;
using LocMp.Order.Application.Orders.Queries.GetCartByUser;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocMp.Order.Api.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class CartsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CartDto?>> GetCart(CancellationToken ct)
    {
        var result = await sender.Send(new GetCartByUserQuery(HttpContext.GetUserId()), ct);
        return Ok(result);
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItem([FromBody] AddToCartRequest request, CancellationToken ct)
    {
        var result = await sender.Send(
            new AddToCartCommand(HttpContext.GetUserId(), request.ProductId, request.Quantity), ct);
        return Ok(result);
    }

    [HttpDelete("items/{cartItemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid cartItemId, CancellationToken ct)
    {
        await sender.Send(new RemoveFromCartCommand(HttpContext.GetUserId(), cartItemId), ct);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart(CancellationToken ct)
    {
        await sender.Send(new ClearCartCommand(HttpContext.GetUserId()), ct);
        return NoContent();
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<OrderDto>> Checkout([FromBody] CheckoutRequest request, CancellationToken ct)
    {
        DeliveryAddressData? address = null;
        if (request.DeliveryAddress is { } da)
            address = new DeliveryAddressData(
                da.City, da.Street, da.HouseNumber, da.Apartment,
                da.Entrance, da.Floor, da.Latitude, da.Longitude,
                da.RecipientName, da.RecipientPhone);

        var result = await sender.Send(
            new CheckoutCommand(HttpContext.GetUserId(), request.DeliveryType, request.BuyerComment, address), ct);
        return CreatedAtAction(nameof(OrdersController.GetById), "Orders", new { id = result.Id }, result);
    }
}
