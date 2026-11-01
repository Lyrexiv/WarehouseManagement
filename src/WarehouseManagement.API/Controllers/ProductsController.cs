using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.Application.Common.Models;
using WarehouseManagement.Application.Features.Products.Commands;
using WarehouseManagement.Application.Features.Products.Queries;
using WarehouseManagement.Application.Features.Products.DTOs;

namespace WarehouseManagement.API.Controllers;

/// <summary>Product management endpoints</summary>
[Tags("Products")]
[Authorize]
public class ProductsController : BaseController
{
    /// <summary>Get all products</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams paging, CancellationToken ct)
        => OkResult(await Mediator.Send(new GetAllProductsQuery(paging), ct));

    /// <summary>Get product by ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => OkResult(await Mediator.Send(new GetProductByIdQuery(id), ct));

    /// <summary>Create product (Admin only)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto, CancellationToken ct)
        => CreatedResult(await Mediator.Send(new CreateProductCommand(dto), ct));

    /// <summary>Update product (Admin only)</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto, CancellationToken ct)
        => OkResult(await Mediator.Send(new UpdateProductCommand(id, dto), ct));

    /// <summary>Delete product (Admin only)</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => OkResult(await Mediator.Send(new DeleteProductCommand(id), ct));
}
