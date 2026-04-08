using LocMp.Identity.Api.Requests.Users;
using LocMp.Identity.Application.DTOs.Common;
using LocMp.Identity.Application.DTOs.User;
using LocMp.Identity.Application.Identity.Commands.Users.BlockUser;
using LocMp.Identity.Application.Identity.Commands.Users.RegisterUser;
using LocMp.Identity.Application.Identity.Commands.Users.UpdateUser;
using LocMp.Identity.Application.Identity.Commands.Users.DeleteUser;
using LocMp.Identity.Application.Identity.Commands.Users.UnblockUser;
using LocMp.Identity.Application.Identity.Commands.Users.UpdateUserRoles;
using LocMp.Identity.Application.Identity.Queries.Users.GetUserByEmail;
using LocMp.Identity.Application.Identity.Queries.Users.GetUserById;
using LocMp.Identity.Application.Identity.Queries.Users.GetUserByPhoneNumber;
using LocMp.Identity.Application.Identity.Queries.Users.GetUserByUsername;
using LocMp.Identity.Application.Identity.Queries.Users.GetUsersByRoleId;
using LocMp.Identity.Application.Identity.Queries.Users.GetUsersPaged;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LocMp.Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers([FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await mediator.Send(new GetUsersPagedQuery(page, pageSize));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id)
    {
        var result = await mediator.Send(new GetUserByIdQuery(id));
        return Ok(result);
    }

    [HttpGet("by-email")]
    public async Task<ActionResult<UserDto>> GetByEmail([FromQuery] string email)
    {
        var result = await mediator.Send(new GetUserByEmailQuery(email));
        return Ok(result);
    }

    [HttpGet("by-role/{roleId:guid}")]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetByRoleId(Guid roleId)
    {
        var result = await mediator.Send(new GetUsersByRoleIdQuery(roleId));
        return Ok(result);
    }

    [HttpGet("by-username")]
    public async Task<ActionResult<UserDto>> GetByUsername([FromQuery] string username)
    {
        var result = await mediator.Send(new GetUserByUsernameQuery(username));
        return Ok(result);
    }

    [HttpGet("by-phone")]
    public async Task<ActionResult<UserDto>> GetByPhone([FromQuery] string phone)
    {
        var result = await mediator.Send(new GetUserByPhoneNumberQuery(phone));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterUserCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserDto>> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var command = new UpdateUserCommand(
            id,
            request.UserName,
            request.Email,
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.Gender,
            request.DateOfBirth,
            request.Active
        );

        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await mediator.Send(new DeleteUserCommand(id));
        return NoContent();
    }

    [HttpPut("{id:guid}/roles")]
    public async Task<ActionResult> UpdateRoles(Guid id, [FromBody] UpdateUserRolesRequest request)
    {
        await mediator.Send(new UpdateUserRolesCommand(id, request.Roles));
        return NoContent();
    }

    [HttpPost("{id:guid}/block")]
    public async Task<IActionResult> BlockUser(Guid id, [FromBody] BlockUserRequest request, CancellationToken ct)
    {
        await mediator.Send(new BlockUserCommand(id, request.DurationInMinutes), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/unblock")]
    public async Task<IActionResult> UnblockUser(Guid id, CancellationToken ct)
    {
        await mediator.Send(new UnblockUserCommand(id), ct);
        return NoContent();
    }
}