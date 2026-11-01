using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.Application.Common.Models;
using WarehouseManagement.Application.Features.Users.DTOs;
using WarehouseManagement.Application.Features.Users.Commands;
using WarehouseManagement.Application.Features.Users.Queries;

namespace WarehouseManagement.API.Controllers;

/// <summary>User management endpoints</summary>
[Tags("Users")]
[Authorize]
public class UsersController : BaseController
{
    /// <summary>Get all users (Admin only)</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams paging, CancellationToken ct)
        => OkResult(await Mediator.Send(new GetAllUsersQuery(paging), ct));

    /// <summary>Get user by ID</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => OkResult(await Mediator.Send(new GetUserByIdQuery(id), ct));

    /// <summary>Create user (Admin only)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto, CancellationToken ct)
        => CreatedResult(await Mediator.Send(new CreateUserCommand(dto), ct));

    /// <summary>Update user (Admin only)</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto, CancellationToken ct)
        => OkResult(await Mediator.Send(new UpdateUserCommand(id, dto), ct));

    /// <summary>Delete user (Admin only)</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => OkResult(await Mediator.Send(new DeleteUserCommand(id), ct));

    /// <summary>Assign roles to user (Admin only)</summary>
    [HttpPost("assign-roles")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRoles([FromBody] AssignRoleDto dto, CancellationToken ct)
        => OkResult(await Mediator.Send(new AssignRoleCommand(dto), ct));
}
