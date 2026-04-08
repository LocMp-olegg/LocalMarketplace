using LocMp.Identity.Api.Requests.Roles;
using LocMp.Identity.Application.DTOs.Role;
using LocMp.Identity.Application.Identity.Commands.Roles.CreateRole;
using LocMp.Identity.Application.Identity.Commands.Roles.UpdateRole;
using LocMp.Identity.Application.Identity.Commands.Roles.DeleteRole;
using LocMp.Identity.Application.Identity.Queries.Roles.GetRoleById;
using LocMp.Identity.Application.Identity.Queries.Roles.GetRoles;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LocMp.Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoleDto>>> GetRoles(CancellationToken ct)
    {
        var result = await sender.Send(new GetRolesQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RoleDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetRoleByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<RoleDto>> Create([FromBody] CreateRoleCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RoleDto>> Update(Guid id, [FromBody] UpdateRoleRequest request, CancellationToken ct)
    {
        var command = new UpdateRoleCommand(id, request.Name, request.Active);
        var result = await sender.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeleteRoleCommand(id), ct);
        return NoContent();
    }
}