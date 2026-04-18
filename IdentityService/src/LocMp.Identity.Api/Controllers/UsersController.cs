using LocMp.BuildingBlocks.Application.Common;
using LocMp.Identity.Api.Requests.Users;
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
public class UsersController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetUsersPagedQuery(page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetUserByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpGet("by-email")]
    public async Task<ActionResult<UserDto>> GetByEmail([FromQuery] string email, CancellationToken ct)
    {
        var result = await sender.Send(new GetUserByEmailQuery(email), ct);
        return Ok(result);
    }

    [HttpGet("by-role/{roleId:guid}")]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetByRoleId(Guid roleId, CancellationToken ct)
    {
        var result = await sender.Send(new GetUsersByRoleIdQuery(roleId), ct);
        return Ok(result);
    }

    [HttpGet("by-username")]
    public async Task<ActionResult<UserDto>> GetByUsername([FromQuery] string username, CancellationToken ct)
    {
        var result = await sender.Send(new GetUserByUsernameQuery(username), ct);
        return Ok(result);
    }

    [HttpGet("by-phone")]
    public async Task<ActionResult<UserDto>> GetByPhone([FromQuery] string phone, CancellationToken ct)
    {
        var result = await sender.Send(new GetUserByPhoneNumberQuery(phone), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterUserCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserDto>> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        var command = new UpdateUserCommand(
            id,
            request.UserName,
            request.Email,
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.Gender,
            request.BirthDate,
            request.Active
        );

        var result = await sender.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeleteUserCommand(id), ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/roles")]
    public async Task<ActionResult> UpdateRoles(Guid id, [FromBody] UpdateUserRolesRequest request,
        CancellationToken ct)
    {
        await sender.Send(new UpdateUserRolesCommand(id, request.Roles), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/block")]
    public async Task<IActionResult> BlockUser(Guid id, [FromBody] BlockUserRequest request, CancellationToken ct)
    {
        await sender.Send(new BlockUserCommand(id, request.DurationInMinutes), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/unblock")]
    public async Task<IActionResult> UnblockUser(Guid id, CancellationToken ct)
    {
        await sender.Send(new UnblockUserCommand(id), ct);
        return NoContent();
    }
}