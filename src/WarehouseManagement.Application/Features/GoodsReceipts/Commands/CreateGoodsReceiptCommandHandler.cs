using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Domain.Enums;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.GoodsReceipts;

public class CreateGoodsReceiptCommandHandler
    : IRequestHandler<CreateGoodsReceiptCommand, GoodsReceiptDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IPermissionService _perm;

    public CreateGoodsReceiptCommandHandler(
        IUnitOfWork uow,
        IPermissionService perm)
    {
        _uow = uow;
        _perm = perm;
    }

    public async Task<GoodsReceiptDto> Handle(
        CreateGoodsReceiptCommand req,
        CancellationToken ct)
    {
        if (!await _perm.CanAccessWarehouseAsync(
                req.CreatedByUserId,
                req.Dto.WarehouseId,
                ct))
        {
            throw new ForbiddenException(
                "Bạn không có quyền tạo phiếu nhập cho kho này.");
        }

        if (!await _perm.HasStaffPermissionAsync(
                req.CreatedByUserId,
                StaffPermission.CreateGoodsReceipt,
                req.Dto.WarehouseId,
                ct))
        {
            throw new ForbiddenException(
                "Bạn không có quyền tạo phiếu nhập kho.");
        }

        var warehouse =
            await _uow.Warehouses.GetByIdAsync(
                req.Dto.WarehouseId,
                ct)
            ?? throw new NotFoundException(
                "Warehouse",
                req.Dto.WarehouseId);

        var supplier =
            await _uow.Suppliers.GetByIdAsync(
                req.Dto.SupplierId,
                ct)
            ?? throw new NotFoundException(
                "Supplier",
                req.Dto.SupplierId);

        var receiptNumber =
            $"RCV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        var receipt = new GoodsReceipt
        {
            ReceiptNumber = receiptNumber,
            WarehouseId = req.Dto.WarehouseId,
            SupplierId = req.Dto.SupplierId,
            ImportDate = req.Dto.ImportDate,
            Note = req.Dto.Note,
            Status = GoodsReceiptStatus.Draft,
            CreatedBy = req.CreatedByUserId.ToString()
        };

        var detailDtos = new List<GoodsReceiptDetailDto>();

        foreach (var d in req.Dto.Details)
        {
            var product =
                await _uow.Products.GetByIdAsync(
                    d.ProductId,
                    ct)
                ?? throw new NotFoundException(
                    "Product",
                    d.ProductId);

            receipt.Details.Add(new GoodsReceiptDetail
            {
                ProductId = d.ProductId,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                Note = d.Note
            });

            detailDtos.Add(new GoodsReceiptDetailDto(
                Guid.Empty,
                d.ProductId,
                product.Name,
                product.SKU,
                d.Quantity,
                d.UnitPrice,
                d.Quantity * d.UnitPrice,
                d.Note));
        }

        receipt.CalculateTotalAmount();

        await _uow.GoodsReceipts.AddAsync(receipt, ct);
        await _uow.SaveChangesAsync(ct);

        return new GoodsReceiptDto(
            receipt.Id,
            receipt.ReceiptNumber,
            receipt.WarehouseId,
            warehouse.Name,
            receipt.SupplierId,
            supplier.Name,
            receipt.ImportDate,
            receipt.TotalAmount,
            receipt.Status.ToString(),
            receipt.Note,
            detailDtos,
            receipt.CreatedAt);
    }
}