namespace WarehouseManagement.Application.Features.Users.DTOs;

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    string FullName,
    string? PhoneNumber,
    bool IsActive,
    IEnumerable<string> Roles,
    DateTime CreatedAt
);

public record CreateUserDto(
    string Username,
    string Email,
    string Password,
    string FullName,
    string? PhoneNumber,
    IEnumerable<Guid> RoleIds
);

public record UpdateUserDto(
    string Email,
    string FullName,
    string? PhoneNumber,
    bool IsActive
);

public record AssignRoleDto(
    Guid UserId,
    IEnumerable<Guid> RoleIds
);