using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Common.Models;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.GoodsReceipts;

public class GetAllGoodsReceiptsQueryHandler
    : IRequestHandler<
        GetAllGoodsReceiptsQuery,
        PagedList<GoodsReceiptDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _me;
    private readonly IPermissionService _perm;

    public GetAllGoodsReceiptsQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService me,
        IPermissionService perm)
    {
        _uow = uow;
        _me = me;
        _perm = perm;
    }

    public async Task<PagedList<GoodsReceiptDto>> Handle(
        GetAllGoodsReceiptsQuery req,
        CancellationToken ct)
    {
        var userId = _me.UserId;

        if (userId is null)
        {
            throw new UnauthorizedException(
                "User is not authenticated");
        }

        var all =
            await _uow.GoodsReceipts.GetAllWithDetailsAsync(ct);

        var query = all.Where(r => !r.IsDeleted);

        if (!_me.IsAdmin)
        {
            var accessibleIds =
                await _perm.GetAccessibleWarehouseIdsAsync(
                    userId.Value,
                    ct);

            if (accessibleIds != null)
            {
                query = query.Where(
                    r => accessibleIds.Contains(r.WarehouseId));
            }
        }

        if (!string.IsNullOrWhiteSpace(req.Params.SearchTerm))
        {
            var t = req.Params.SearchTerm.ToLower();

            query = query.Where(r =>
                r.ReceiptNumber.ToLower().Contains(t)
                || (r.Supplier != null
                    && r.Supplier.Name.ToLower().Contains(t)));
        }

        var total = query.Count();

        var items = query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((req.Params.PageNumber - 1) * req.Params.PageSize)
            .Take(req.Params.PageSize)
            .Select(r => new GoodsReceiptDto(
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
                r.CreatedAt))
            .ToList();

        return new PagedList<GoodsReceiptDto>(
            items,
            total,
            req.Params.PageNumber,
            req.Params.PageSize);
    }
}