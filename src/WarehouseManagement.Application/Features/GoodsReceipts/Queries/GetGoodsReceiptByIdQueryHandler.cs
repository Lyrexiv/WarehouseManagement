using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.GoodsReceipts;

public class GetGoodsReceiptByIdQueryHandler
    : IRequestHandler<
        GetGoodsReceiptByIdQuery,
        GoodsReceiptDto>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _me;
    private readonly IPermissionService _perm;

    public GetGoodsReceiptByIdQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService me,
        IPermissionService perm)
    {
        _uow = uow;
        _me = me;
        _perm = perm;
    }

    public async Task<GoodsReceiptDto> Handle(
        GetGoodsReceiptByIdQuery req,
        CancellationToken ct)
    {
        var userId = _me.UserId;

        if (userId is null)
        {
            throw new UnauthorizedException(
                "User is not authenticated");
        }

        var receipt =
            await _uow.GoodsReceipts.GetWithDetailsAsync(
                req.Id,
                ct)
            ?? throw new NotFoundException(
                "GoodsReceipt",
                req.Id);

        if (!await _perm.CanAccessWarehouseAsync(
                userId.Value,
                receipt.WarehouseId,
                ct))
        {
            throw new ForbiddenException(
                "Bạn không có quyền xem phiếu nhập này.");
        }

        return new GoodsReceiptDto(
            receipt.Id,
            receipt.ReceiptNumber,
            receipt.WarehouseId,
            receipt.Warehouse?.Name ?? "",
            receipt.SupplierId,
            receipt.Supplier?.Name ?? "",
            receipt.ImportDate,
            receipt.TotalAmount,
            receipt.Status.ToString(),
            receipt.Note,
            receipt.Details.Select(d =>
                new GoodsReceiptDetailDto(
                    d.Id,
                    d.ProductId,
                    d.Product?.Name ?? "",
                    d.Product?.SKU ?? "",
                    d.Quantity,
                    d.UnitPrice,
                    d.TotalPrice,
                    d.Note)),
            receipt.CreatedAt);
    }
}