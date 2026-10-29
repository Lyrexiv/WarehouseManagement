using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.Application.Features.Suppliers;

namespace WarehouseManagement.API.Controllers;

/// <summary>Supplier management endpoints</summary>
[Tags("Suppliers")]
[Authorize]
public class SuppliersController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => OkResult(await Mediator.Send(new GetAllSuppliersQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => OkResult(await Mediator.Send(new GetSupplierByIdQuery(id), ct));

    [HttpPost]
    [Authorize(Roles = "Admin,WarehouseManager")]
    public async Task<IActionResult> Create([FromBody] CreateSupplierDto dto, CancellationToken ct)
        => CreatedResult(await Mediator.Send(new CreateSupplierCommand(dto), ct));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,WarehouseManager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplierDto dto, CancellationToken ct)
        => OkResult(await Mediator.Send(new UpdateSupplierCommand(id, dto), ct));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => OkResult(await Mediator.Send(new DeleteSupplierCommand(id), ct));
}
