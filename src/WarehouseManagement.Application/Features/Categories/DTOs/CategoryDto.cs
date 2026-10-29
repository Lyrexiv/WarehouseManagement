namespace WarehouseManagement.Application.Features.Categories.DTOs;

public record CategoryDto(
    Guid Id,
    string Name,
    string? Description,
    string? Code,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateCategoryDto(
    string Name,
    string? Description,
    string? Code
);

public record UpdateCategoryDto(
    string Name,
    string? Description,
    string? Code,
    bool IsActive
);