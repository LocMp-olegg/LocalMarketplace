using LocMp.Catalog.Application.Catalog.Queries.Sellers.GetSeller;
using LocMp.Catalog.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LocMp.Catalog.Api.Controllers;

[ApiController]
[Route("api/sellers")]
public sealed class SellersController(ISender sender) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SellerDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetSellerQuery(id), ct));
}
