using MediatR;

namespace WarehouseManagement.Application.Features.GoodsReceipts;

public record GetGoodsReceiptByIdQuery(Guid Id)
    : IRequest<GoodsReceiptDto>;