using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.Permissions;
using WarehouseManagement.Domain.Entities;

namespace WarehouseManagement.API.Controllers;

/// <summary>
/// Quản lý phân quyền — Admin only
/// </summary>
[ApiController]
[Route("api/permissions")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class PermissionsController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _me;

    public PermissionsController(
        ISender mediator,
        ICurrentUserService me)
    {
        _mediator = mediator;
        _me = me;
    }

    // ================================================================
    // WAREHOUSE ASSIGNMENT (Manager)
    // ================================================================

    /// <summary>Phân công kho cho Manager</summary>
    [HttpPost("warehouses/assign")]
    public async Task<IActionResult> AssignWarehouse(
        [FromBody] AssignWarehouseRequest req,
        CancellationToken ct)
    {
        if (!_me.UserId.HasValue)
            return Unauthorized();

        var result = await _mediator.Send(
            new AssignWarehouseCommand(
                req.ManagerUserId,
                req.WarehouseId,
                req.Note,
                _me.UserId.Value),
            ct);

        return Ok(new
        {
            success = true,
            data = result
        });
    }

    /// <summary>Thu hồi phân công kho của Manager</summary>
    [HttpPost("warehouses/revoke")]
    public async Task<IActionResult> RevokeWarehouse(
        [FromBody] RevokeWarehouseRequest req,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new RevokeWarehouseCommand(
                req.UserId,
                req.WarehouseId),
            ct);

        return Ok(new
        {
            success = true,
            data = result
        });
    }

    /// <summary>Xem danh sách kho được phân công của 1 Manager</summary>
    [HttpGet("warehouses/by-user/{userId:guid}")]
    public async Task<IActionResult> GetAssignmentsByUser(
        Guid userId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetAssignmentsByUserQuery(userId),
            ct);

        return Ok(new
        {
            success = true,
            data = result
        });
    }

    /// <summary>Xem danh sách Manager quản lý 1 kho</summary>
    [HttpGet("warehouses/by-warehouse/{warehouseId:guid}")]
    public async Task<IActionResult> GetAssignmentsByWarehouse(
        Guid warehouseId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetAssignmentsByWarehouseQuery(warehouseId),
            ct);

        return Ok(new
        {
            success = true,
            data = result
        });
    }

    // ================================================================
    // STAFF PERMISSIONS
    // ================================================================

    /// <summary>Cap 1 quyen cho Staff (tuy chon gioi han theo kho)</summary>
    [HttpPost("staff/grant")]
    public async Task<IActionResult> GrantPermission(
        [FromBody] GrantPermissionRequest req,
        CancellationToken ct)
    {
        if (!_me.UserId.HasValue)
            return Unauthorized();

        var result = await _mediator.Send(
            new GrantStaffPermissionCommand(
                req.StaffUserId,
                req.Permission,
                req.WarehouseId,
                _me.UserId.Value),
            ct);

        return Ok(new
        {
            success = true,
            data = result
        });
    }

    /// <summary>Cap nhieu quyen cho Staff trong 1 lan goi</summary>
    [HttpPost("staff/grant-bulk")]
    public async Task<IActionResult> BulkGrantPermission(
        [FromBody] BulkGrantRequest req,
        CancellationToken ct)
    {
        if (!_me.UserId.HasValue)
            return Unauthorized();

        var result = await _mediator.Send(
            new BulkGrantPermissionCommand(
                req.StaffUserId,
                req.Permissions,
                req.WarehouseId,
                _me.UserId.Value),
            ct);

        return Ok(new
        {
            success = true,
            data = result
        });
    }

    /// <summary>Thu hoi 1 quyen cu the (theo GrantId)</summary>
    [HttpDelete("staff/revoke/{grantId:guid}")]
    public async Task<IActionResult> RevokePermission(
        Guid grantId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new RevokeStaffPermissionCommand(grantId),
            ct);

        return Ok(new
        {
            success = true,
            data = result
        });
    }

    /// <summary>Thu hoi tat ca quyen cua 1 Staff</summary>
    [HttpPost("staff/revoke-all/{userId:guid}")]
    public async Task<IActionResult> RevokeAllPermissions(
        Guid userId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new RevokeAllStaffPermissionsCommand(userId),
            ct);

        return Ok(new
        {
            success = true,
            data = result
        });
    }

    /// <summary>Xem tat ca quyen cua 1 Staff</summary>
    [HttpGet("staff/{userId:guid}")]
    public async Task<IActionResult> GetStaffPermissions(
        Guid userId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetStaffPermissionsQuery(userId),
            ct);

        return Ok(new
        {
            success = true,
            data = result
        });
    }

    /// <summary>Kiem tra 1 user co quyen nao do khong</summary>
    [HttpGet("check")]
    public async Task<IActionResult> Check(
        [FromQuery] Guid userId,
        [FromQuery] StaffPermission permission,
        [FromQuery] Guid? warehouseId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CheckPermissionQuery(
                userId,
                permission,
                warehouseId),
            ct);

        return Ok(new
        {
            success = true,
            data = result
        });
    }

    /// <summary>Lay tat ca quyen nghiep vu co the cap</summary>
    [HttpGet("available-permissions")]
    public IActionResult GetAvailablePermissions()
    {
        var perms = Enum.GetValues<StaffPermission>()
            .Select(p => new
            {
                value = (int)p,
                name = p.ToString(),
                description = GetPermissionDescription(p),
                group = GetPermissionGroup(p)
            })
            .GroupBy(p => p.group)
            .Select(g => new
            {
                group = g.Key,
                permissions = g.ToList()
            });

        return Ok(new
        {
            success = true,
            data = perms
        });
    }

    // ================================================================
    // MY PERMISSIONS
    // ================================================================

    /// <summary>Xem kho duoc phan cong / quyen cua ban than</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyPermissions(
        CancellationToken ct)
    {
        if (!_me.UserId.HasValue)
            return Unauthorized();

        var userId = _me.UserId.Value;

        if (_me.IsAdmin)
        {
            return Ok(new
            {
                success = true,
                data = new
                {
                    role = "Admin",
                    access = "Toàn quyền hệ thống"
                }
            });
        }

        if (_me.IsManager)
        {
            var assignments = await _mediator.Send(
                new GetAssignmentsByUserQuery(userId),
                ct);

            return Ok(new
            {
                success = true,
                data = new
                {
                    role = "WarehouseManager",
                    assignments
                }
            });
        }

        var perms = await _mediator.Send(
            new GetStaffPermissionsQuery(userId),
            ct);

        return Ok(new
        {
            success = true,
            data = new
            {
                role = "Staff",
                permissions = perms
            }
        });
    }

    // ================================================================
    // HELPERS
    // ================================================================

    private static string GetPermissionDescription(
        StaffPermission p) => p switch
    {
        StaffPermission.ViewGoodsReceipts =>
            "Xem phiếu nhập kho",

        StaffPermission.CreateGoodsReceipt =>
            "Tạo phiếu nhập kho",

        StaffPermission.ApproveGoodsReceipt =>
            "Duyệt phiếu nhập kho",

        StaffPermission.CancelGoodsReceipt =>
            "Hủy phiếu nhập kho",

        StaffPermission.ViewGoodsIssues =>
            "Xem phiếu xuất kho",

        StaffPermission.CreateGoodsIssue =>
            "Tạo phiếu xuất kho",

        StaffPermission.ApproveGoodsIssue =>
            "Duyệt phiếu xuất kho",

        StaffPermission.CancelGoodsIssue =>
            "Hủy phiếu xuất kho",

        StaffPermission.ViewInventory =>
            "Xem tồn kho",

        StaffPermission.AdjustStock =>
            "Điều chỉnh tồn kho",

        StaffPermission.ViewAudit =>
            "Xem kiểm kê",

        StaffPermission.CreateAudit =>
            "Tạo đợt kiểm kê",

        StaffPermission.CompleteAudit =>
            "Hoàn thành kiểm kê",

        StaffPermission.ViewReports =>
            "Xem báo cáo",

        StaffPermission.ViewProducts =>
            "Xem sản phẩm",

        StaffPermission.ViewCategories =>
            "Xem danh mục",

        StaffPermission.ViewSuppliers =>
            "Xem nhà cung cấp",

        _ => p.ToString()
    };

    private static string GetPermissionGroup(
        StaffPermission p) => (int)p switch
    {
        >= 10 and < 20 => "Phiếu nhập kho",
        >= 20 and < 30 => "Phiếu xuất kho",
        >= 30 and < 40 => "Tồn kho",
        >= 40 and < 50 => "Kiểm kê",
        >= 50 and < 60 => "Báo cáo",
        >= 60 => "Xem danh mục",
        _ => "Khác"
    };
}

// ================================================================
// REQUEST MODELS
// ================================================================

public record AssignWarehouseRequest(
    Guid ManagerUserId,
    Guid WarehouseId,
    string? Note);

public record RevokeWarehouseRequest(
    Guid UserId,
    Guid WarehouseId);

public record GrantPermissionRequest(
    Guid StaffUserId,
    StaffPermission Permission,
    Guid? WarehouseId);

public record BulkGrantRequest(
    Guid StaffUserId,
    IEnumerable<StaffPermission> Permissions,
    Guid? WarehouseId);