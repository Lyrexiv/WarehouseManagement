namespace WarehouseManagement.Application.Features.Suppliers.DTOs;

public record SupplierDto(
    Guid Id,
    string Code,
    string Name,
    string? ContactPerson,
    string? Email,
    string? PhoneNumber,
    string? Address,
    string? TaxCode,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateSupplierDto(
    string Code,
    string Name,
    string? ContactPerson,
    string? Email,
    string? PhoneNumber,
    string? Address,
    string? TaxCode
);

public record UpdateSupplierDto(
    string Code,
    string Name,
    string? ContactPerson,
    string? Email,
    string? PhoneNumber,
    string? Address,
    string? TaxCode,
    bool IsActive
);