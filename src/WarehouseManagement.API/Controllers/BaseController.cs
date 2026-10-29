using MediatR;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.Application.Common.Models;

namespace WarehouseManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IActionResult OkResult<T>(T data, string? message = null) =>
        Ok(ApiResponse<T>.Ok(data, message));

    protected IActionResult CreatedResult<T>(T data, string? message = null) =>
        StatusCode(201, ApiResponse<T>.Ok(data, message ?? "Created successfully"));
}
