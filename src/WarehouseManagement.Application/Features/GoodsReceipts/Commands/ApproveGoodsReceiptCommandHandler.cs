using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Domain.Enums;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.GoodsReceipts;

public class ApproveGoodsReceiptCommandHandler
    : IRequestHandler<ApproveGoodsReceiptCommand, GoodsReceiptDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IPermissionService _perm;

    public ApproveGoodsReceiptCommandHandler(
        IUnitOfWork uow,
        IPermissionService perm)
    {
        _uow = uow;
        _perm = perm;
    }

    public async Task<GoodsReceiptDto> Handle(
        ApproveGoodsReceiptCommand req,
        CancellationToken ct)
    {
        var receipt =
            await _uow.GoodsReceipts.GetWithDetailsAsync(
                req.Id,
                ct)
            ?? throw new NotFoundException(
                "GoodsReceipt",
                req.Id);

        if (!await _perm.CanAccessWarehouseAsync(
                req.ApprovedByUserId,
                receipt.WarehouseId,
                ct))
        {
            throw new ForbiddenException(
                "Bạn không có quyền duyệt phiếu nhập cho kho này.");
        }

        if (!await _perm.HasStaffPermissionAsync(
                req.ApprovedByUserId,
                StaffPermission.ApproveGoodsReceipt,
                receipt.WarehouseId,
                ct))
        {
            throw new ForbiddenException(
                "Bạn không có quyền duyệt phiếu nhập kho.");
        }

        if (receipt.Status != GoodsReceiptStatus.Draft)
        {
            throw new BusinessException(
                "Chỉ duyệt được phiếu ở trạng thái Nháp.");
        }

        await _uow.BeginTransactionAsync(ct);

        try
        {
            receipt.Status = GoodsReceiptStatus.Approved;
            receipt.ApprovedByUserId = req.ApprovedByUserId;
            receipt.ApprovedAt = DateTime.UtcNow;

            foreach (var detail in receipt.Details)
            {
                var inv =
                    await _uow.Inventories
                        .GetByWarehouseAndProductAsync(
                            receipt.WarehouseId,
                            detail.ProductId,
                            ct);

                if (inv == null)
                {
                    inv = new Inventory
                    {
                        WarehouseId = receipt.WarehouseId,
                        ProductId = detail.ProductId,
                        Quantity = 0
                    };

                    await _uow.Inventories.AddAsync(inv, ct);
                    await _uow.SaveChangesAsync(ct);
                }

                var qtyBefore = inv.Quantity;

                inv.AddStock(detail.Quantity);

                await _uow.Inventories.UpdateAsync(inv, ct);

                await _uow.StockMovements.AddAsync(
                    new StockMovement
                    {
                        WarehouseId = receipt.WarehouseId,
                        ProductId = detail.ProductId,
                        MovementType = MovementType.Receipt,
                        Quantity = detail.Quantity,
                        QuantityBefore = qtyBefore,
                        QuantityAfter = inv.Quantity,
                        ReferenceNumber = receipt.ReceiptNumber,
                        CreatedByUserId = req.ApprovedByUserId
                    },
                    ct);
            }

            await _uow.GoodsReceipts.UpdateAsync(receipt, ct);
            await _uow.SaveChangesAsync(ct);

            await _uow.CommitTransactionAsync(ct);

            return MapToDto(receipt);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    private static GoodsReceiptDto MapToDto(GoodsReceipt r)
    {
        return new GoodsReceiptDto(
            r.Id,
            r.ReceiptNumber,
            r.WarehouseId,
            r.Warehouse?.Name ?? "",
            r.SupplierId,
            r.Supplier?.Name ?? "",
            r.ImportDate,
            r.TotalAmount,
            r.Status.ToString(),
            r.Note,
            r.Details.Select(d =>
                new GoodsReceiptDetailDto(
                    d.Id,
                    d.ProductId,
                    d.Product?.Name ?? "",
                    d.Product?.SKU ?? "",
                    d.Quantity,
                    d.UnitPrice,
                    d.TotalPrice,
                    d.Note)),
            r.CreatedAt);
    }
}