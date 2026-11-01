using MediatR;
using WarehouseManagement.Domain.Enums;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.GoodsReceipts;

public record CancelGoodsReceiptCommand(
   Guid Id)
   : IRequest<bool>;

