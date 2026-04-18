using LocMp.Order.Api.Extensions;
using LocMp.Order.Api.Requests;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Application.Orders.Commands.Cart.AddToCart;
using LocMp.Order.Application.Orders.Commands.Cart.ClearCart;
using LocMp.Order.Application.Orders.Commands.Cart.RemoveFromCart;
using LocMp.Order.Application.Orders.Commands.Cart.UpdateCartItemQuantity;
using LocMp.Order.Application.Orders.Commands.Orders.Checkout;
using LocMp.Order.Application.Orders.Queries.Cart.GetCartByUser;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocMp.Order.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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

    [HttpPut("items/{cartItemId:guid}")]
    public async Task<ActionResult<CartDto>> UpdateItemQuantity(
        Guid cartItemId, [FromBody] UpdateCartItemQuantityRequest request, CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateCartItemQuantityCommand(HttpContext.GetUserId(), cartItemId, request.Quantity), ct);
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
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> Checkout(
        [FromBody] CheckoutRequest request, CancellationToken ct)
    {
        var groups = request.Groups.Select(g =>
        {
            DeliveryAddressData? addr = null;
            if (g.DeliveryAddress is { } da)
                addr = new DeliveryAddressData(
                    da.City, da.Street, da.HouseNumber, da.Apartment,
                    da.Entrance, da.Floor, da.Latitude, da.Longitude,
                    da.RecipientName, da.RecipientPhone);
            return new GroupDeliverySettings(g.SellerId, g.ShopId, g.DeliveryType, addr, g.SelectedItemIds);
        }).ToList();

        var result = await sender.Send(
            new CheckoutCommand(HttpContext.GetUserId(), request.BuyerComment, groups), ct);
        return Ok(result);
    }
}