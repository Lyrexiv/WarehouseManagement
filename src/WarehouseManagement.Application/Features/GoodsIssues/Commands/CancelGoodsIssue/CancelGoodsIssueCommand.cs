using MediatR;

namespace WarehouseManagement.Application.Features.GoodsIssues.Commands.CancelGoodsIssue;

public record CancelGoodsIssueCommand(
    Guid Id)
    : IRequest<bool>;