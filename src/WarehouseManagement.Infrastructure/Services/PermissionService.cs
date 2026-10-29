using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly IUnitOfWork _uow;

    public PermissionService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    // ================================================================
    // ROLE CHECKS
    // ================================================================

    public async Task<bool> IsAdminAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var user = await _uow.Users.GetWithRolesAsync(userId, ct);

        return user?.UserRoles.Any(
            r => r.Role.Name == "Admin") ?? false;
    }

    public async Task<bool> IsManagerAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var user = await _uow.Users.GetWithRolesAsync(userId, ct);

        return user?.UserRoles.Any(
            r => r.Role.Name == "WarehouseManager") ?? false;
    }

    public async Task<bool> IsInRoleAsync(
        Guid userId,
        string role,
        CancellationToken ct = default)
    {
        var user = await _uow.Users.GetWithRolesAsync(userId, ct);

        return user?.UserRoles.Any(
            r => r.Role.Name == role) ?? false;
    }

    // ================================================================
    // WAREHOUSE ACCESS
    // ================================================================

    /// <summary>
    /// Admin: truy cập tất cả kho.
    /// WarehouseManager: chỉ truy cập kho được phân công.
    /// Staff: cần có ít nhất một quyền đang hoạt động trên kho đó,
    /// hoặc có quyền toàn cục (WarehouseId = null).
    /// </summary>
    public async Task<bool> CanAccessWarehouseAsync(
        Guid userId,
        Guid warehouseId,
        CancellationToken ct = default)
    {
        if (await IsAdminAsync(userId, ct))
            return true;

        if (await IsManagerAsync(userId, ct))
        {
            return await _uow.WarehouseAssignments.IsAssignedAsync(
                userId,
                warehouseId,
                ct);
        }

        var grants = await _uow.StaffPermissions.GetByUserAsync(
            userId,
            ct);

        return grants.Any(g =>
            g.WarehouseId == null ||
            g.WarehouseId == warehouseId);
    }

    /// <summary>
    /// null: Admin hoặc người dùng có quyền toàn cục trên tất cả kho.
    /// Manager: danh sách kho được phân công.
    /// Staff: danh sách kho có quyền.
    /// </summary>
    public async Task<IReadOnlyList<Guid>?> GetAccessibleWarehouseIdsAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        if (await IsAdminAsync(userId, ct))
            return null;

        if (await IsManagerAsync(userId, ct))
        {
            return await _uow.WarehouseAssignments
                .GetAssignedWarehouseIdsAsync(userId, ct);
        }

        var grants = await _uow.StaffPermissions.GetByUserAsync(
            userId,
            ct);

        // Có quyền toàn cục trên mọi kho.
        if (grants.Any(g => g.WarehouseId == null))
            return null;

        return grants
            .Where(g => g.WarehouseId.HasValue)
            .Select(g => g.WarehouseId!.Value)
            .Distinct()
            .ToList();
    }

    // ================================================================
    // STAFF PERMISSION
    // ================================================================

    /// <summary>
    /// Admin: luôn có quyền.
    /// WarehouseManager: chỉ có quyền khi được phân công kho cụ thể.
    /// Staff: cần có grant tương ứng, theo kho hoặc toàn cục.
    ///
    /// Lưu ý:
    /// - Khi warehouseId có giá trị, Manager phải được phân công kho đó.
    /// - Khi warehouseId là null, Manager không được mặc định có quyền
    ///   trên mọi kho; cần kiểm tra riêng quyền toàn cục nếu nghiệp vụ
    ///   của hệ thống cho phép.
    /// </summary>
    public async Task<bool> HasStaffPermissionAsync(
        Guid userId,
        StaffPermission permission,
        Guid? warehouseId = null,
        CancellationToken ct = default)
    {
        if (await IsAdminAsync(userId, ct))
            return true;

        if (await IsManagerAsync(userId, ct))
        {
            // Không cho phép suy ra quyền trên mọi kho
            // chỉ vì người dùng có vai trò WarehouseManager.
            if (!warehouseId.HasValue)
                return false;

            return await _uow.WarehouseAssignments.IsAssignedAsync(
                userId,
                warehouseId.Value,
                ct);
        }

        return await _uow.StaffPermissions.HasPermissionAsync(
            userId,
            permission,
            warehouseId,
            ct);
    }
}