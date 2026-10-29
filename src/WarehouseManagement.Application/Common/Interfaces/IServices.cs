using WarehouseManagement.Domain.Entities;
namespace WarehouseManagement.Application.Common.Interfaces;


// ================================================================
// PERMISSION SERVICE — business logic kiem tra quyen
// ================================================================
public interface IPermissionService
{
    /// <summary>Admin: true. Manager: phan cong. Staff: co quyen tuong ung.</summary>
    Task<bool> CanAccessWarehouseAsync(Guid userId, Guid warehouseId, CancellationToken ct = default);

    /// <summary>Lay danh sach kho user co quyen (Admin = null = tat ca).</summary>
    Task<IReadOnlyList<Guid>?> GetAccessibleWarehouseIdsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Kiem tra Staff co nghiep vu (va tuy chon theo kho).</summary>
    Task<bool> HasStaffPermissionAsync(Guid userId, StaffPermission permission, Guid? warehouseId = null, CancellationToken ct = default);

    /// <summary>Kiem tra user co role khong.</summary>
    Task<bool> IsInRoleAsync(Guid userId, string role, CancellationToken ct = default);

    /// <summary>Tra ve true neu la Admin.</summary>
    Task<bool> IsAdminAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Tra ve true neu la Manager (co phan cong).</summary>
    Task<bool> IsManagerAsync(Guid userId, CancellationToken ct = default);
}

// ================================================================
// CURRENT USER SERVICE — extended voi permission
// ================================================================
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string Username { get; }
    string Email { get; }
    IEnumerable<string> Roles { get; }

    bool IsInRole(string role);
    bool IsAdmin { get; }
    bool IsManager { get; }
    bool IsStaff { get; }
}
public interface IJwtService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    string GenerateRefreshToken();
    Guid? GetUserIdFromToken(string token);
}
public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string token, CancellationToken ct = default);
    Task SendWelcomeEmailAsync(string email, string username, CancellationToken ct = default);
}
public interface IDateTimeService
{
    DateTime UtcNow { get; }
}
