using MediatR;

using WarehouseManagement.Application.Features.GoodsIssues.DTOs;

namespace WarehouseManagement.Application.Features.GoodsIssues.Commands.CreateGoodsIssue;

public record CreateGoodsIssueCommand(
    CreateGoodsIssueDto Dto,
    Guid CreatedByUserId)
    : IRequest<GoodsIssueDto>;