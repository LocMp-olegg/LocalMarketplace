using LocMp.Catalog.Application.Catalog.Queries.Tags.GetAllTags;
using LocMp.Catalog.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LocMp.Catalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TagsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TagDto>>> GetAll(CancellationToken ct)
        => Ok(await sender.Send(new GetAllTagsQuery(), ct));
}
