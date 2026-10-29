using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.API.Filters;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Common.Models;
using WarehouseManagement.Application.Features.GoodsReceipts;
using WarehouseManagement.Domain.Entities;

namespace WarehouseManagement.API.Controllers;

/// <summary>
/// Goods Receipt (Phieu nhap kho) endpoints
/// </summary>
[Tags("Goods Receipts - Nhap Kho")]
[Authorize]
public class GoodsReceiptsController : BaseController
{
    private readonly ICurrentUserService _currentUser;

    public GoodsReceiptsController(ICurrentUserService currentUser)
        => _currentUser = currentUser;

    /// <summary>
    /// Get all goods receipts
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginationParams paging,
        CancellationToken ct)
        => OkResult(
            await Mediator.Send(
                new GetAllGoodsReceiptsQuery(paging),
                ct));

    /// <summary>
    /// Get goods receipt by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken ct)
        => OkResult(
            await Mediator.Send(
                new GetGoodsReceiptByIdQuery(id),
                ct));

    /// <summary>
    /// Create a goods receipt (draft).
    /// Staff requires CreateGoodsReceipt permission.
    /// </summary>
    [HttpPost]
    [RequireStaffPermission(StaffPermission.CreateGoodsReceipt)]
    public async Task<IActionResult> Create(
        [FromBody] CreateGoodsReceiptDto dto,
        CancellationToken ct)
    {
        var userId = _currentUser.UserId;

        if (!userId.HasValue)
            return Unauthorized();

        return CreatedResult(
            await Mediator.Send(
                new CreateGoodsReceiptCommand(
                    dto,
                    userId.Value),
                ct));
    }

    /// <summary>
    /// Approve goods receipt and update inventory.
    /// Staff requires ApproveGoodsReceipt permission.
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [RequireStaffPermission(StaffPermission.ApproveGoodsReceipt)]
    public async Task<IActionResult> Approve(
        Guid id,
        CancellationToken ct)
    {
        var userId = _currentUser.UserId;

        if (!userId.HasValue)
            return Unauthorized();

        return OkResult(
            await Mediator.Send(
                new ApproveGoodsReceiptCommand(
                    id,
                    userId.Value),
                ct));
    }

    /// <summary>
    /// Cancel goods receipt.
    /// Staff requires CancelGoodsReceipt permission.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [RequireStaffPermission(StaffPermission.CancelGoodsReceipt)]
    public async Task<IActionResult> Cancel(
        Guid id,
        CancellationToken ct)
        => OkResult(
            await Mediator.Send(
                new CancelGoodsReceiptCommand(id),
                ct));
}