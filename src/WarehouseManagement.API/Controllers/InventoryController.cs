using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.API.Filters;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.Inventories;
using WarehouseManagement.Application.Features.InventoryAudits;
using WarehouseManagement.Application.Features.StockAdjustments;
using WarehouseManagement.Domain.Entities;

namespace WarehouseManagement.API.Controllers;

/// <summary>
/// Inventory management endpoints
/// </summary>
[Tags("Inventory - Ton Kho")]
[Authorize]
public class InventoryController : BaseController
{
    private readonly ICurrentUserService _currentUser;

    public InventoryController(ICurrentUserService currentUser)
        => _currentUser = currentUser;

    /// <summary>
    /// Xem tồn kho theo kho.
    /// Staff cần quyền ViewInventory.
    /// </summary>
    [HttpGet("by-warehouse/{warehouseId:guid}")]
    [RequireStaffPermission(
        StaffPermission.ViewInventory,
        "warehouseId")]
    public async Task<IActionResult> GetByWarehouse(
        Guid warehouseId,
        CancellationToken ct)
        => OkResult(
            await Mediator.Send(
                new GetInventoryByWarehouseQuery(warehouseId),
                ct));

    /// <summary>
    /// Xem tồn kho theo sản phẩm.
    /// Việc lọc warehouse phải xử lý trong Query Handler.
    /// </summary>
    [HttpGet("by-product/{productId:guid}")]
    [RequireStaffPermission(StaffPermission.ViewInventory)]
    public async Task<IActionResult> GetByProduct(
        Guid productId,
        CancellationToken ct)
        => OkResult(
            await Mediator.Send(
                new GetInventoryByProductQuery(productId),
                ct));

    /// <summary>
    /// Xem cảnh báo hàng sắp hết.
    /// </summary>
    [HttpGet("low-stock")]
    [RequireStaffPermission(StaffPermission.ViewInventory)]
    public async Task<IActionResult> GetLowStock(
        CancellationToken ct)
        => OkResult(
            await Mediator.Send(
                new GetLowStockQuery(),
                ct));

    /// <summary>
    /// Điều chỉnh tồn kho.
    /// </summary>
    [HttpPost("adjust")]
    [RequireStaffPermission(StaffPermission.AdjustStock)]
    public async Task<IActionResult> Adjust(
        [FromBody] CreateStockAdjustmentDto dto,
        CancellationToken ct)
    {
        var userId = _currentUser.UserId;

        if (!userId.HasValue)
            return Unauthorized();

        return CreatedResult(
            await Mediator.Send(
                new CreateStockAdjustmentCommand(
                    dto,
                    userId.Value),
                ct));
    }

    /// <summary>
    /// Tạo đợt kiểm kê.
    /// </summary>
    [HttpPost("audit")]
    [RequireStaffPermission(StaffPermission.CreateAudit)]
    public async Task<IActionResult> CreateAudit(
        [FromBody] CreateInventoryAuditDto dto,
        CancellationToken ct)
        => CreatedResult(
            await Mediator.Send(
                new CreateInventoryAuditCommand(dto),
                ct));

    /// <summary>
    /// Lấy danh sách các đợt kiểm kê.
    /// </summary>
    [HttpGet("audits")]
    [RequireStaffPermission(StaffPermission.ViewInventory)]
    public async Task<IActionResult> GetAudits(
        [FromQuery] Guid? warehouseId,
        CancellationToken ct)
        => OkResult(
            await Mediator.Send(
                new GetAllInventoryAuditsQuery(warehouseId),
                ct));

    /// <summary>
    /// Hoàn thành kiểm kê.
    /// </summary>
    [HttpPost("audit/{auditId:guid}/complete")]
    [RequireStaffPermission(StaffPermission.CompleteAudit)]
    public async Task<IActionResult> CompleteAudit(
        Guid auditId,
        [FromBody] IEnumerable<AddAuditDetailDto> actualCounts,
        CancellationToken ct)
        => OkResult(
            await Mediator.Send(
                new CompleteInventoryAuditCommand(
                    auditId,
                    actualCounts),
                ct));
}