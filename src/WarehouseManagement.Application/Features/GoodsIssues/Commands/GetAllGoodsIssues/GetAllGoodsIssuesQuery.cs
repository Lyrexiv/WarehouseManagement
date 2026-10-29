using MediatR;

using WarehouseManagement.Application.Common.Models;
using WarehouseManagement.Application.Features.GoodsIssues.DTOs;

namespace WarehouseManagement.Application.Features.GoodsIssues.Queries.GetAllGoodsIssues;

public record GetAllGoodsIssuesQuery(
    PaginationParams Params)
    : IRequest<PagedList<GoodsIssueDto>>;