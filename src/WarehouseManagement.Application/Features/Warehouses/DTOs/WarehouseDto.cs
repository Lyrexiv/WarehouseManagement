namespace WarehouseManagement.Application.Features.Warehouses.DTOs;

public record WarehouseDto(
    Guid Id,
    string Code,
    string Name,
    string Address,
    string? City,
    string? Country,
    decimal Capacity,
    string? Description,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateWarehouseDto(
    string Code,
    string Name,
    string Address,
    string? City,
    string? Country,
    decimal Capacity,
    string? Description
);

public record UpdateWarehouseDto(
    string Code,
    string Name,
    string Address,
    string? City,
    string? Country,
    decimal Capacity,
    string? Description,
    bool IsActive
);