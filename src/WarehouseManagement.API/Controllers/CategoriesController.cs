using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.Application.Features.Categories.Commands.CreateCategory;
using WarehouseManagement.Application.Features.Categories.Commands.UpdateCategory;
using WarehouseManagement.Application.Features.Categories.Commands.DeleteCategory;
using WarehouseManagement.Application.Features.Categories.DTOs;
using WarehouseManagement.Application.Features.Categories.Queries;

namespace WarehouseManagement.API.Controllers;

/// <summary>Category management endpoints</summary>
[Tags("Categories")]
[Authorize]
public class CategoriesController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => OkResult(await Mediator.Send(new GetAllCategoriesQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => OkResult(await Mediator.Send(new GetCategoryByIdQuery(id), ct));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto, CancellationToken ct)
        => CreatedResult(await Mediator.Send(new CreateCategoryCommand(dto), ct));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryDto dto, CancellationToken ct)
        => OkResult(await Mediator.Send(new UpdateCategoryCommand(id, dto), ct));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => OkResult(await Mediator.Send(new DeleteCategoryCommand(id), ct));
}
