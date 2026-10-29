namespace WarehouseManagement.Application.Features.Auth.DTOs;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserInfo User
);

public record UserInfo(
    Guid Id,
    string Username,
    string Email,
    string FullName,
    IEnumerable<string> Roles
);