using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManagement.Domain.Enums;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.GoodsReceipts.Commands
{
    public class CancelGoodsReceiptCommandHandler
    : IRequestHandler<CancelGoodsReceiptCommand, bool>
    {
        private readonly IUnitOfWork _uow;

        public CancelGoodsReceiptCommandHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<bool> Handle(
            CancelGoodsReceiptCommand req,
            CancellationToken ct)
        {
            var receipt =
                await _uow.GoodsReceipts.GetByIdAsync(
                    req.Id,
                    ct)
                ?? throw new NotFoundException(
                    "GoodsReceipt",
                    req.Id);

            if (receipt.Status != GoodsReceiptStatus.Draft)
            {
                throw new BusinessException(
                    "Chỉ hủy được phiếu ở trạng thái Nháp.");
            }

            receipt.Status = GoodsReceiptStatus.Cancelled;
            receipt.UpdatedAt = DateTime.UtcNow;

            await _uow.GoodsReceipts.UpdateAsync(receipt, ct);
            await _uow.SaveChangesAsync(ct);

            return true;
        }
    }
}
