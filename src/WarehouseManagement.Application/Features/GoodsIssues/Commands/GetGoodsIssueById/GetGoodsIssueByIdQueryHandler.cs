using MediatR;

using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.GoodsIssues.DTOs;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.GoodsIssues.Queries.GetGoodsIssueById;

public class GetGoodsIssueByIdQueryHandler
    : IRequestHandler<
        GetGoodsIssueByIdQuery,
        GoodsIssueDto>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _me;
    private readonly IPermissionService _perm;

    public GetGoodsIssueByIdQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService me,
        IPermissionService perm)
    {
        _uow = uow;
        _me = me;
        _perm = perm;
    }

    public async Task<GoodsIssueDto> Handle(
        GetGoodsIssueByIdQuery req,
        CancellationToken ct)
    {
        // ----------------------------------------------------
        // Current user
        // ----------------------------------------------------

        var userId = _me.UserId;

        if (userId is null)
        {
            throw new UnauthorizedException(
                "User is not authenticated");
        }

        // ----------------------------------------------------
        // Get issue
        // ----------------------------------------------------

        var issue =
            await _uow.GoodsIssues
                .GetWithDetailsAsync(
                    req.Id,
                    ct)
            ?? throw new NotFoundException(
                "GoodsIssue",
                req.Id);

        // ----------------------------------------------------
        // Check warehouse access
        // ----------------------------------------------------

        if (!await _perm.CanAccessWarehouseAsync(
                userId.Value,
                issue.WarehouseId,
                ct))
        {
            throw new ForbiddenException(
                "Bạn không có quyền xem phiếu xuất này.");
        }

        // ----------------------------------------------------
        // Return DTO
        // ----------------------------------------------------

        return new GoodsIssueDto(
            issue.Id,
            issue.IssueNumber,
            issue.WarehouseId,
            issue.Warehouse?.Name ?? "",

            issue.ExportDate,
            issue.ReceiverName,
            issue.ReceiverDepartment,
            issue.Purpose,

            issue.Status.ToString(),
            issue.Note,

            issue.Details.Select(
                d => new GoodsIssueDetailDto(
                    d.Id,
                    d.ProductId,
                    d.Product?.Name ?? "",
                    d.Product?.SKU ?? "",
                    d.Quantity,
                    d.UnitPrice,
                    d.TotalPrice,
                    d.Note)),

            issue.CreatedAt);
    }
}