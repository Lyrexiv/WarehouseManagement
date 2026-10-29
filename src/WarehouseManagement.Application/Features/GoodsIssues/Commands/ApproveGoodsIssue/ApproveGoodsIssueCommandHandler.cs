using MediatR;

using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.GoodsIssues.DTOs;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Domain.Enums;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.GoodsIssues.Commands.ApproveGoodsIssue;

public class ApproveGoodsIssueCommandHandler
    : IRequestHandler<ApproveGoodsIssueCommand, GoodsIssueDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IPermissionService _perm;

    public ApproveGoodsIssueCommandHandler(
        IUnitOfWork uow,
        IPermissionService perm)
    {
        _uow = uow;
        _perm = perm;
    }

    public async Task<GoodsIssueDto> Handle(
        ApproveGoodsIssueCommand req,
        CancellationToken ct)
    {
        // ----------------------------------------------------
        // Get issue
        // ----------------------------------------------------

        var issue =
            await _uow.GoodsIssues
                .GetWithDetailsAsync(
                    req.Id,
                    ct)
            ?? throw new NotFoundException(
                "GoodsIssue",
                req.Id);

        // ----------------------------------------------------
        // Check warehouse access
        // ----------------------------------------------------

        if (!await _perm.CanAccessWarehouseAsync(
                req.ApprovedByUserId,
                issue.WarehouseId,
                ct))
        {
            throw new ForbiddenException(
                "Bạn không có quyền duyệt phiếu xuất cho kho này.");
        }

        // ----------------------------------------------------
        // Check approve permission
        // ----------------------------------------------------

        if (!await _perm.HasStaffPermissionAsync(
                req.ApprovedByUserId,
                StaffPermission.ApproveGoodsIssue,
                issue.WarehouseId,
                ct))
        {
            throw new ForbiddenException(
                "Bạn không có quyền duyệt phiếu xuất kho.");
        }

        // ----------------------------------------------------
        // Check status
        // ----------------------------------------------------

        if (issue.Status != GoodsIssueStatus.Draft)
        {
            throw new BusinessException(
                "Chỉ duyệt được phiếu ở trạng thái Nháp.");
        }

        // ----------------------------------------------------
        // Transaction
        // ----------------------------------------------------

        await _uow.BeginTransactionAsync(ct);

        try
        {
            // -----------------------------------------------
            // Update issue status
            // -----------------------------------------------

            issue.Status =
                GoodsIssueStatus.Approved;

            issue.ApprovedByUserId =
                req.ApprovedByUserId;

            issue.ApprovedAt =
                DateTime.UtcNow;

            // -----------------------------------------------
            // Remove stock
            // -----------------------------------------------

            foreach (var detail in issue.Details)
            {
                var inv =
                    await _uow.Inventories
                        .GetByWarehouseAndProductAsync(
                            issue.WarehouseId,
                            detail.ProductId,
                            ct)
                    ?? throw new BusinessException(
                        $"Không tìm thấy tồn kho sản phẩm {detail.ProductId}");

                // -------------------------------------------
                // Check stock
                // -------------------------------------------

                if (inv.AvailableQuantity < detail.Quantity)
                {
                    throw new InsufficientStockException(
                        detail.Product?.Name ?? "",
                        detail.Quantity,
                        inv.AvailableQuantity);
                }

                // -------------------------------------------
                // Save quantity before
                // -------------------------------------------

                var qtyBefore =
                    inv.Quantity;

                // -------------------------------------------
                // Remove stock
                // -------------------------------------------

                inv.RemoveStock(
                    detail.Quantity);

                await _uow.Inventories.UpdateAsync(
                    inv,
                    ct);

                // -------------------------------------------
                // Stock movement
                // -------------------------------------------

                await _uow.StockMovements.AddAsync(
                    new StockMovement
                    {
                        WarehouseId =
                            issue.WarehouseId,

                        ProductId =
                            detail.ProductId,

                        MovementType =
                            MovementType.Issue,

                        Quantity =
                            -detail.Quantity,

                        QuantityBefore =
                            qtyBefore,

                        QuantityAfter =
                            inv.Quantity,

                        ReferenceNumber =
                            issue.IssueNumber,

                        CreatedByUserId =
                            req.ApprovedByUserId
                    },
                    ct);
            }

            // ------------------------------------------------
            // Update GoodsIssue
            // ------------------------------------------------

            await _uow.GoodsIssues.UpdateAsync(
                issue,
                ct);

            await _uow.SaveChangesAsync(ct);

            // ------------------------------------------------
            // Commit
            // ------------------------------------------------

            await _uow.CommitTransactionAsync(ct);

            return MapToDto(issue);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);

            throw;
        }
    }

    private static GoodsIssueDto MapToDto(
        GoodsIssue i)
    {
        return new GoodsIssueDto(
            i.Id,
            i.IssueNumber,
            i.WarehouseId,
            i.Warehouse?.Name ?? "",
            i.ExportDate,
            i.ReceiverName,
            i.ReceiverDepartment,
            i.Purpose,
            i.Status.ToString(),
            i.Note,

            i.Details.Select(
                d => new GoodsIssueDetailDto(
                    d.Id,
                    d.ProductId,
                    d.Product?.Name ?? "",
                    d.Product?.SKU ?? "",
                    d.Quantity,
                    d.UnitPrice,
                    d.TotalPrice,
                    d.Note)),

            i.CreatedAt);
    }
}