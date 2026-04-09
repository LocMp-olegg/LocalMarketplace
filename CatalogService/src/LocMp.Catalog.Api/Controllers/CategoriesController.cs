using LocMp.Catalog.Api.Requests.Categories;
using LocMp.Catalog.Application.Catalog.Commands.Categories.CreateCategory;
using LocMp.Catalog.Application.Catalog.Commands.Categories.DeleteCategory;
using LocMp.Catalog.Application.Catalog.Commands.Categories.UpdateCategory;
using LocMp.Catalog.Application.Catalog.Commands.Categories.UploadCategoryImage;
using LocMp.Catalog.Application.Catalog.Queries.Categories.GetCategoriesTree;
using LocMp.Catalog.Application.Catalog.Queries.Categories.GetCategoryById;
using LocMp.Catalog.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocMp.Catalog.Api.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController(ISender sender) : ControllerBase
{
    [HttpGet("tree")]
    public async Task<ActionResult<IReadOnlyList<CategoryTreeDto>>> GetTree(CancellationToken ct)
        => Ok(await sender.Send(new GetCategoriesTreeQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetCategoryByIdQuery(id), ct));

    [HttpPost("{id:guid}/image")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<string>> UploadImage(Guid id, IFormFile image, CancellationToken ct)
        => Ok(await sender.Send(new UploadCategoryImageCommand(id, image), ct));

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<CategoryDto>> Create([FromForm] CreateCategoryRequest request, CancellationToken ct)
    {
        var command = new CreateCategoryCommand(
            request.ParentCategoryId,
            request.Name,
            request.Description,
            request.Image,
            request.SortOrder
        );
        var result = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<CategoryDto>> Update(Guid id, [FromForm] UpdateCategoryRequest request,
        CancellationToken ct)
    {
        var command = new UpdateCategoryCommand(
            id,
            request.Name,
            request.Description,
            request.Image,
            request.SortOrder,
            request.IsActive
        );
        return Ok(await sender.Send(command, ct));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeleteCategoryCommand(id), ct);
        return NoContent();
    }
}
