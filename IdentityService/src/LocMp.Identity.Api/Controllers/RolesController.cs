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
public class RolesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoleDto>>> GetRoles()
    {
        var result = await mediator.Send(new GetRolesQuery());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RoleDto>> GetById(Guid id)
    {
        var result = await mediator.Send(new GetRoleByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<RoleDto>> Create([FromBody] CreateRoleCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RoleDto>> Update(Guid id, [FromBody] UpdateRoleRequest request)
    {
        var command = new UpdateRoleCommand(
            id,
            request.Name,
            request.Active
        );

        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await mediator.Send(new DeleteRoleCommand(id));
        return NoContent();
    }
}