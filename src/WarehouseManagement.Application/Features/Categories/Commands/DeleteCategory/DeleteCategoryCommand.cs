using MediatR;

namespace WarehouseManagement.Application.Features.Categories.Commands.DeleteCategory;

public record DeleteCategoryCommand(
    Guid Id
) : IRequest<bool>;