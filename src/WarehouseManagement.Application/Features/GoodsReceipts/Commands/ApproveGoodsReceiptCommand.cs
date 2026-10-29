using MediatR;

namespace WarehouseManagement.Application.Features.GoodsReceipts;

public record ApproveGoodsReceiptCommand(
    Guid Id,
    Guid ApprovedByUserId)
    : IRequest<GoodsReceiptDto>;