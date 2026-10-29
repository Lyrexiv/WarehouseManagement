using MediatR;

using WarehouseManagement.Application.Features.GoodsIssues.DTOs;

namespace WarehouseManagement.Application.Features.GoodsIssues.Queries.GetGoodsIssueById;

public record GetGoodsIssueByIdQuery(
    Guid Id)
    : IRequest<GoodsIssueDto>;