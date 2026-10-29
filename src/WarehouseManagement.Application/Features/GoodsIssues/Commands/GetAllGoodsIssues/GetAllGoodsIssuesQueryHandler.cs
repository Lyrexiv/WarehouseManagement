using MediatR;

using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Common.Models;
using WarehouseManagement.Application.Features.GoodsIssues.DTOs;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.GoodsIssues.Queries.GetAllGoodsIssues;

public class GetAllGoodsIssuesQueryHandler
    : IRequestHandler<
        GetAllGoodsIssuesQuery,
        PagedList<GoodsIssueDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _me;
    private readonly IPermissionService _perm;

    public GetAllGoodsIssuesQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService me,
        IPermissionService perm)
    {
        _uow = uow;
        _me = me;
        _perm = perm;
    }

    public async Task<PagedList<GoodsIssueDto>> Handle(
        GetAllGoodsIssuesQuery req,
        CancellationToken ct)
    {
        // ----------------------------------------------------
        // Get current user from JWT
        // ----------------------------------------------------

        var userId = _me.UserId;

        if (userId is null)
        {
            throw new UnauthorizedException(
                "User is not authenticated");
        }

        // ----------------------------------------------------
        // Get all
        // ----------------------------------------------------

        var all =
            await _uow.GoodsIssues
                .GetAllWithDetailsAsync(ct);

        // ----------------------------------------------------
        // Không dùng AsQueryable()
        // ----------------------------------------------------

        var query =
            all.Where(i => !i.IsDeleted);

        // ----------------------------------------------------
        // Warehouse permission
        // ----------------------------------------------------

        if (!_me.IsAdmin)
        {
            var accessibleIds =
                await _perm.GetAccessibleWarehouseIdsAsync(
                    userId.Value,
                    ct);

            if (accessibleIds != null)
            {
                query =
                    query.Where(
                        i => accessibleIds.Contains(
                            i.WarehouseId));
            }
        }

        // ----------------------------------------------------
        // Search
        // ----------------------------------------------------

        if (!string.IsNullOrWhiteSpace(
                req.Params.SearchTerm))
        {
            var t =
                req.Params.SearchTerm
                    .ToLower();

            query =
                query.Where(i =>
                    i.IssueNumber
                        .ToLower()
                        .Contains(t)
                    ||
                    i.ReceiverName
                        .ToLower()
                        .Contains(t));
        }

        // ----------------------------------------------------
        // Total
        // ----------------------------------------------------

        var total =
            query.Count();

        // ----------------------------------------------------
        // Pagination
        // ----------------------------------------------------

        var items =
            query
                .OrderByDescending(
                    i => i.CreatedAt)

                .Skip(
                    (req.Params.PageNumber - 1)
                    * req.Params.PageSize)

                .Take(
                    req.Params.PageSize)

                .Select(i =>
                    new GoodsIssueDto(
                        i.Id,
                        i.IssueNumber,
                        i.WarehouseId,
                        i.Warehouse?.Name ?? "",

                        i.ExportDate,
                        i.ReceiverName,
                        i.ReceiverDepartment,
                        i.Purpose,

                        i.Status.ToString(),
                        i.Note,

                        i.Details.Select(d =>
                            new GoodsIssueDetailDto(
                                d.Id,
                                d.ProductId,
                                d.Product?.Name ?? "",
                                d.Product?.SKU ?? "",
                                d.Quantity,
                                d.UnitPrice,
                                d.TotalPrice,
                                d.Note)),

                        i.CreatedAt))
                .ToList();

        // ----------------------------------------------------
        // Return
        // ----------------------------------------------------

        return new PagedList<GoodsIssueDto>(
            items,
            total,
            req.Params.PageNumber,
            req.Params.PageSize);
    }
}