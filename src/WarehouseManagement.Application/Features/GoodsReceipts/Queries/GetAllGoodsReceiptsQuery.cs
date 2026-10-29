using MediatR;
using WarehouseManagement.Application.Common.Models;

namespace WarehouseManagement.Application.Features.GoodsReceipts;

public record GetAllGoodsReceiptsQuery(
    PaginationParams Params)
    : IRequest<PagedList<GoodsReceiptDto>>;