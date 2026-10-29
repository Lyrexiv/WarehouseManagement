using MediatR;

using WarehouseManagement.Application.Features.GoodsIssues.DTOs;

namespace WarehouseManagement.Application.Features.GoodsIssues.Commands.ApproveGoodsIssue;

public record ApproveGoodsIssueCommand(
    Guid Id,
    Guid ApprovedByUserId)
    : IRequest<GoodsIssueDto>;