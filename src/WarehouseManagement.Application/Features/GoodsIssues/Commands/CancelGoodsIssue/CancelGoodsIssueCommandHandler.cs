using MediatR;

using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Enums;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.GoodsIssues.Commands.CancelGoodsIssue;

public class CancelGoodsIssueCommandHandler
    : IRequestHandler<CancelGoodsIssueCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public CancelGoodsIssueCommandHandler(
        IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(
        CancelGoodsIssueCommand req,
        CancellationToken ct)
    {
        // ----------------------------------------------------
        // Get issue
        // ----------------------------------------------------

        var issue =
            await _uow.GoodsIssues
                .GetByIdAsync(
                    req.Id,
                    ct)
            ?? throw new NotFoundException(
                "GoodsIssue",
                req.Id);

        // ----------------------------------------------------
        // Only Draft can be cancelled
        // ----------------------------------------------------

        if (issue.Status != GoodsIssueStatus.Draft)
        {
            throw new BusinessException(
                "Chỉ hủy được phiếu ở trạng thái Nháp.");
        }

        // ----------------------------------------------------
        // Cancel
        // ----------------------------------------------------

        issue.Status =
            GoodsIssueStatus.Cancelled;

        issue.UpdatedAt =
            DateTime.UtcNow;

        // ----------------------------------------------------
        // Save
        // ----------------------------------------------------

        await _uow.GoodsIssues.UpdateAsync(
            issue,
            ct);

        await _uow.SaveChangesAsync(ct);

        return true;
    }
}