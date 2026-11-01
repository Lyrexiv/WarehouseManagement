using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.Application.Common.Models;
using WarehouseManagement.Application.Features.Warehouses.DTOs;
using WarehouseManagement.Application.Features.Warehouses.Commands;
using WarehouseManagement.Application.Features.Warehouses.Queries;

namespace WarehouseManagement.API.Controllers;

/// <summary>Warehouse management endpoints</summary>
[Tags("Warehouses")]
[Authorize]
public class WarehousesController : BaseController
{
    /// <summary>Get all warehouses</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams paging, CancellationToken ct)
        => OkResult(await Mediator.Send(new GetAllWarehousesQuery(paging), ct));

    /// <summary>Get warehouse by ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => OkResult(await Mediator.Send(new GetWarehouseByIdQuery(id), ct));

    /// <summary>Create warehouse (Admin only)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseDto dto, CancellationToken ct)
        => CreatedResult(await Mediator.Send(new CreateWarehouseCommand(dto), ct));

    /// <summary>Update warehouse (Admin only)</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWarehouseDto dto, CancellationToken ct)
        => OkResult(await Mediator.Send(new UpdateWarehouseCommand(id, dto), ct));

    /// <summary>Delete warehouse (Admin only)</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => OkResult(await Mediator.Send(new DeleteWarehouseCommand(id), ct));
}
