using MediatR;

namespace WarehouseManagement.Application.Features.GoodsReceipts;

public record CreateGoodsReceiptCommand(
    CreateGoodsReceiptDto Dto,
    Guid CreatedByUserId)
    : IRequest<GoodsReceiptDto>;