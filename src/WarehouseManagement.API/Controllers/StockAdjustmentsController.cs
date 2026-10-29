using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.StockAdjustments;

namespace WarehouseManagement.API.Controllers;

/// <summary>
/// Điều chỉnh tồn kho
/// </summary>
[ApiController]
[Route("api/stockadjustments")]
[Produces("application/json")]
[Authorize]
public class StockAdjustmentsController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUser;

    public StockAdjustmentsController(
        ISender mediator,
        ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Lấy danh sách điều chỉnh.
    /// Có thể lọc theo kho / sản phẩm.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? warehouseId,
        [FromQuery] Guid? productId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetAllStockAdjustmentsQuery(
                warehouseId,
                productId),
            ct);

        return Ok(new
        {
            success = true,
            data = result
        });
    }

    /// <summary>
    /// Tạo phiếu điều chỉnh và cập nhật tồn kho ngay.
    /// Admin và WarehouseManager được phép thực hiện.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,WarehouseManager")]
    public async Task<IActionResult> Create(
        [FromBody] CreateStockAdjustmentDto dto,
        CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Unauthorized();

        var userId = _currentUser.UserId.Value;

        var result = await _mediator.Send(
            new CreateStockAdjustmentCommand(
                dto,
                userId),
            ct);

        return StatusCode(
            StatusCodes.Status201Created,
            new
            {
                success = true,
                data = result
            });
    }
}