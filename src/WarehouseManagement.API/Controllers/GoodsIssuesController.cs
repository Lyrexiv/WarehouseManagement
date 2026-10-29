using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.API.Filters;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Common.Models;
using WarehouseManagement.Application.Features.GoodsIssues.DTOs;
using WarehouseManagement.Application.Features.GoodsIssues.Commands.CreateGoodsIssue;
using WarehouseManagement.Application.Features.GoodsIssues.Commands.ApproveGoodsIssue;
using WarehouseManagement.Application.Features.GoodsIssues.Commands.CancelGoodsIssue;
using WarehouseManagement.Application.Features.GoodsIssues.Queries.GetAllGoodsIssues;
using WarehouseManagement.Application.Features.GoodsIssues.Queries.GetGoodsIssueById;
using WarehouseManagement.Domain.Entities;

namespace WarehouseManagement.API.Controllers;

/// <summary>
/// Goods Issue (Phieu xuat kho) endpoints
/// </summary>
[Tags("Goods Issues - Xuat Kho")]
[Authorize]
public class GoodsIssuesController : BaseController
{
    private readonly ICurrentUserService _currentUser;

    public GoodsIssuesController(ICurrentUserService currentUser)
        => _currentUser = currentUser;

    /// <summary>
    /// Get all goods issues
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginationParams paging,
        CancellationToken ct)
        => OkResult(
            await Mediator.Send(
                new GetAllGoodsIssuesQuery(paging),
                ct));

    /// <summary>
    /// Get goods issue by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken ct)
        => OkResult(
            await Mediator.Send(
                new GetGoodsIssueByIdQuery(id),
                ct));

    /// <summary>
    /// Create goods issue (draft).
    /// Staff requires CreateGoodsIssue permission.
    /// </summary>
    [HttpPost]
    [RequireStaffPermission(StaffPermission.CreateGoodsIssue)]
    public async Task<IActionResult> Create(
        [FromBody] CreateGoodsIssueDto dto,
        CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Unauthorized("User is not authenticated.");

        return CreatedResult(
            await Mediator.Send(
                new CreateGoodsIssueCommand(
                    dto,
                    _currentUser.UserId.Value),
                ct));
    }

    /// <summary>
    /// Approve goods issue and deduct inventory.
    /// Staff requires ApproveGoodsIssue permission.
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [RequireStaffPermission(StaffPermission.ApproveGoodsIssue)]
    public async Task<IActionResult> Approve(
        Guid id,
        CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Unauthorized("User is not authenticated.");

        return OkResult(
            await Mediator.Send(
                new ApproveGoodsIssueCommand(
                    id,
                    _currentUser.UserId.Value),
                ct));
    }

    /// <summary>
    /// Cancel goods issue (draft only).
    /// Staff requires CancelGoodsIssue permission.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [RequireStaffPermission(StaffPermission.CancelGoodsIssue)]
    public async Task<IActionResult> Cancel(
        Guid id,
        CancellationToken ct)
        => OkResult(
            await Mediator.Send(
                new CancelGoodsIssueCommand(id),
                ct));
}