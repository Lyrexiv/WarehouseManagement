using FluentValidation;
using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Permissions;

// ================================================================
// DTOs
// ================================================================

public record WarehouseAssignmentDto(
    Guid Id,
    Guid UserId,
    string UserFullName,
    string Username,
    Guid WarehouseId,
    string WarehouseName,
    string WarehouseCode,
    bool IsActive,
    DateTime AssignedAt,
    string? Note);

public record StaffPermissionGrantDto(
    Guid Id,
    Guid UserId,
    string UserFullName,
    string Permission,
    int PermissionValue,
    Guid? WarehouseId,
    string? WarehouseName,
    bool IsActive,
    DateTime GrantedAt);

// ================================================================
// ASSIGN WAREHOUSE TO MANAGER
// ================================================================

public record AssignWarehouseCommand(
    Guid ManagerUserId,
    Guid WarehouseId,
    string? Note,
    Guid AssignedByUserId)
    : IRequest<WarehouseAssignmentDto>;

public class AssignWarehouseCommandValidator
    : AbstractValidator<AssignWarehouseCommand>
{
    public AssignWarehouseCommandValidator()
    {
        RuleFor(x => x.ManagerUserId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.AssignedByUserId).NotEmpty();
    }
}

public class AssignWarehouseCommandHandler
    : IRequestHandler<AssignWarehouseCommand, WarehouseAssignmentDto>
{
    private readonly IUnitOfWork _uow;

    public AssignWarehouseCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<WarehouseAssignmentDto> Handle(
        AssignWarehouseCommand req,
        CancellationToken ct)
    {
        var user = await _uow.Users.GetWithRolesAsync(
            req.ManagerUserId, ct)
            ?? throw new NotFoundException(
                "User", req.ManagerUserId);

        if (!user.UserRoles.Any(
                r => r.Role.Name == "WarehouseManager"))
        {
            throw new BusinessException(
                "Chỉ có thể phân công kho cho WarehouseManager.");
        }

        var warehouse = await _uow.Warehouses.GetByIdAsync(
            req.WarehouseId, ct)
            ?? throw new NotFoundException(
                "Warehouse", req.WarehouseId);

        var existing =
            await _uow.WarehouseAssignments.FirstOrDefaultAsync(
                x => x.UserId == req.ManagerUserId
                  && x.WarehouseId == req.WarehouseId,
                ct);

        if (existing != null)
        {
            existing.IsActive = true;
            existing.IsDeleted = false;
            existing.Note = req.Note;
            existing.UpdatedAt = DateTime.UtcNow;

            await _uow.WarehouseAssignments.UpdateAsync(
                existing, ct);

            await _uow.SaveChangesAsync(ct);

            return MapDto(existing, user, warehouse);
        }

        var assignment = new WarehouseAssignment
        {
            UserId = req.ManagerUserId,
            WarehouseId = req.WarehouseId,
            AssignedByUserId = req.AssignedByUserId,
            Note = req.Note,
            IsActive = true,
            IsDeleted = false
        };

        await _uow.WarehouseAssignments.AddAsync(
            assignment, ct);

        await _uow.SaveChangesAsync(ct);

        return MapDto(assignment, user, warehouse);
    }

    private static WarehouseAssignmentDto MapDto(
        WarehouseAssignment assignment,
        User user,
        Warehouse warehouse)
    {
        return new WarehouseAssignmentDto(
            assignment.Id,
            assignment.UserId,
            user.FullName,
            user.Username,
            assignment.WarehouseId,
            warehouse.Name,
            warehouse.Code,
            assignment.IsActive,
            assignment.AssignedAt,
            assignment.Note);
    }
}

// ================================================================
// REVOKE WAREHOUSE FROM MANAGER
// ================================================================

public record RevokeWarehouseCommand(
    Guid UserId,
    Guid WarehouseId) : IRequest<bool>;

public class RevokeWarehouseCommandHandler
    : IRequestHandler<RevokeWarehouseCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public RevokeWarehouseCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(
        RevokeWarehouseCommand req,
        CancellationToken ct)
    {
        var assignment =
            await _uow.WarehouseAssignments.FirstOrDefaultAsync(
                x => x.UserId == req.UserId
                  && x.WarehouseId == req.WarehouseId,
                ct)
            ?? throw new NotFoundException(
                "WarehouseAssignment",
                $"User={req.UserId}, Warehouse={req.WarehouseId}");

        assignment.IsActive = false;
        assignment.UpdatedAt = DateTime.UtcNow;

        await _uow.WarehouseAssignments.UpdateAsync(
            assignment, ct);

        await _uow.SaveChangesAsync(ct);

        return true;
    }
}

// ================================================================
// GET WAREHOUSE ASSIGNMENTS
// ================================================================

public record GetAssignmentsByUserQuery(Guid UserId)
    : IRequest<IEnumerable<WarehouseAssignmentDto>>;

public record GetAssignmentsByWarehouseQuery(Guid WarehouseId)
    : IRequest<IEnumerable<WarehouseAssignmentDto>>;

public class GetAssignmentsByUserQueryHandler
    : IRequestHandler<
        GetAssignmentsByUserQuery,
        IEnumerable<WarehouseAssignmentDto>>
{
    private readonly IUnitOfWork _uow;

    public GetAssignmentsByUserQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<WarehouseAssignmentDto>> Handle(
        GetAssignmentsByUserQuery req,
        CancellationToken ct)
    {
        var user = await _uow.Users.GetByIdAsync(
            req.UserId, ct)
            ?? throw new NotFoundException("User", req.UserId);

        var assignments =
            await _uow.WarehouseAssignments.GetByUserAsync(
                req.UserId, ct);

        return assignments.Select(a =>
            new WarehouseAssignmentDto(
                a.Id,
                a.UserId,
                user.FullName,
                user.Username,
                a.WarehouseId,
                a.Warehouse?.Name ?? string.Empty,
                a.Warehouse?.Code ?? string.Empty,
                a.IsActive,
                a.AssignedAt,
                a.Note));
    }
}

public class GetAssignmentsByWarehouseQueryHandler
    : IRequestHandler<
        GetAssignmentsByWarehouseQuery,
        IEnumerable<WarehouseAssignmentDto>>
{
    private readonly IUnitOfWork _uow;

    public GetAssignmentsByWarehouseQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<WarehouseAssignmentDto>> Handle(
        GetAssignmentsByWarehouseQuery req,
        CancellationToken ct)
    {
        var assignments =
            await _uow.WarehouseAssignments.GetByWarehouseAsync(
                req.WarehouseId, ct);

        return assignments.Select(a =>
            new WarehouseAssignmentDto(
                a.Id,
                a.UserId,
                a.User?.FullName ?? string.Empty,
                a.User?.Username ?? string.Empty,
                a.WarehouseId,
                a.Warehouse?.Name ?? string.Empty,
                a.Warehouse?.Code ?? string.Empty,
                a.IsActive,
                a.AssignedAt,
                a.Note));
    }
}

// ================================================================
// GRANT STAFF PERMISSION
// ================================================================

public record GrantStaffPermissionCommand(
    Guid StaffUserId,
    StaffPermission Permission,
    Guid? WarehouseId,
    Guid GrantedByUserId)
    : IRequest<StaffPermissionGrantDto>;

public class GrantStaffPermissionCommandValidator
    : AbstractValidator<GrantStaffPermissionCommand>
{
    public GrantStaffPermissionCommandValidator()
    {
        RuleFor(x => x.StaffUserId).NotEmpty();
        RuleFor(x => x.GrantedByUserId).NotEmpty();
    }
}

public class GrantStaffPermissionCommandHandler
    : IRequestHandler<
        GrantStaffPermissionCommand,
        StaffPermissionGrantDto>
{
    private readonly IUnitOfWork _uow;

    public GrantStaffPermissionCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<StaffPermissionGrantDto> Handle(
        GrantStaffPermissionCommand req,
        CancellationToken ct)
    {
        // 1. Kiểm tra Staff tồn tại.
        var user = await _uow.Users.GetWithRolesAsync(
            req.StaffUserId, ct)
            ?? throw new NotFoundException(
                "User", req.StaffUserId);

        // 2. Chỉ cấp quyền cho tài khoản có role Staff.
        if (!user.UserRoles.Any(r => r.Role.Name == "Staff"))
        {
            throw new BusinessException(
                "Chỉ có thể cấp quyền cho Staff.");
        }

        // 3. Kiểm tra kho nếu cấp quyền theo kho cụ thể.
        Warehouse? warehouse = null;

        if (req.WarehouseId.HasValue)
        {
            warehouse = await _uow.Warehouses.GetByIdAsync(
                req.WarehouseId.Value, ct)
                ?? throw new NotFoundException(
                    "Warehouse", req.WarehouseId.Value);
        }

        // 4. Tìm bản ghi đã tồn tại, kể cả khi bị vô hiệu hóa
        //    hoặc xóa mềm.
        var existing = await _uow.StaffPermissions.FindExistingAsync(
            req.StaffUserId,
            req.Permission,
            req.WarehouseId,
            ct);

        if (existing != null)
        {
            // 5. Khôi phục quyền cũ thay vì INSERT bản ghi mới.
            existing.IsActive = true;
            existing.IsDeleted = false;
            existing.GrantedByUserId = req.GrantedByUserId;
            existing.UpdatedAt = DateTime.UtcNow;

            if (warehouse != null)
            {
                existing.Warehouse = warehouse;
            }

            await _uow.StaffPermissions.UpdateAsync(existing, ct);
            await _uow.SaveChangesAsync(ct);

            return MapDto(existing, user);
        }

        // 6. Chỉ INSERT khi thực sự chưa có bản ghi tương ứng.
        var grant = new StaffPermissionGrant
        {
            UserId = req.StaffUserId,
            Permission = req.Permission,
            WarehouseId = req.WarehouseId,
            Warehouse = warehouse,
            GrantedByUserId = req.GrantedByUserId,
            IsActive = true,
            IsDeleted = false
        };

        await _uow.StaffPermissions.AddAsync(grant, ct);
        await _uow.SaveChangesAsync(ct);

        return MapDto(grant, user);
    }

    private static StaffPermissionGrantDto MapDto(
        StaffPermissionGrant grant,
        User user)
    {
        return new StaffPermissionGrantDto(
            grant.Id,
            grant.UserId,
            user.FullName,
            grant.Permission.ToString(),
            (int)grant.Permission,
            grant.WarehouseId,
            grant.Warehouse?.Name,
            grant.IsActive,
            grant.GrantedAt);
    }
}
// ================================================================
// BULK GRANT STAFF PERMISSIONS
// ================================================================

public record BulkGrantPermissionCommand(
    Guid StaffUserId,
    IEnumerable<StaffPermission> Permissions,
    Guid? WarehouseId,
    Guid GrantedByUserId)
    : IRequest<IEnumerable<StaffPermissionGrantDto>>;

public class BulkGrantPermissionCommandValidator
    : AbstractValidator<BulkGrantPermissionCommand>
{
    public BulkGrantPermissionCommandValidator()
    {
        RuleFor(x => x.StaffUserId).NotEmpty();
        RuleFor(x => x.GrantedByUserId).NotEmpty();

        RuleFor(x => x.Permissions)
            .NotNull()
            .Must(p => p.Any())
            .WithMessage("Phải chọn ít nhất một quyền.");
    }
}

public class BulkGrantPermissionCommandHandler
    : IRequestHandler<
        BulkGrantPermissionCommand,
        IEnumerable<StaffPermissionGrantDto>>
{
    private readonly ISender _mediator;

    public BulkGrantPermissionCommandHandler(ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IEnumerable<StaffPermissionGrantDto>> Handle(
        BulkGrantPermissionCommand req,
        CancellationToken ct)
    {
        var results = new List<StaffPermissionGrantDto>();

        // Loại bỏ quyền bị lặp trong cùng request.
        foreach (var permission in req.Permissions.Distinct())
        {
            var result = await _mediator.Send(
                new GrantStaffPermissionCommand(
                    req.StaffUserId,
                    permission,
                    req.WarehouseId,
                    req.GrantedByUserId),
                ct);

            results.Add(result);
        }

        return results;
    }
}

// ================================================================
// REVOKE ONE STAFF PERMISSION
// ================================================================

public record RevokeStaffPermissionCommand(Guid GrantId)
    : IRequest<bool>;

public class RevokeStaffPermissionCommandHandler
    : IRequestHandler<RevokeStaffPermissionCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public RevokeStaffPermissionCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(
        RevokeStaffPermissionCommand req,
        CancellationToken ct)
    {
        var grant = await _uow.StaffPermissions.GetByIdAsync(
            req.GrantId, ct)
            ?? throw new NotFoundException(
                "StaffPermissionGrant", req.GrantId);

        grant.IsActive = false;
        grant.UpdatedAt = DateTime.UtcNow;

        await _uow.StaffPermissions.UpdateAsync(
            grant, ct);

        await _uow.SaveChangesAsync(ct);

        return true;
    }
}

// ================================================================
// REVOKE ALL ACTIVE STAFF PERMISSIONS
// ================================================================

public record RevokeAllStaffPermissionsCommand(Guid StaffUserId)
    : IRequest<bool>;

public class RevokeAllStaffPermissionsCommandHandler
    : IRequestHandler<
        RevokeAllStaffPermissionsCommand,
        bool>
{
    private readonly IUnitOfWork _uow;

    public RevokeAllStaffPermissionsCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(
        RevokeAllStaffPermissionsCommand req,
        CancellationToken ct)
    {
        var grants = await _uow.StaffPermissions.GetByUserAsync(
            req.StaffUserId, ct);

        foreach (var grant in grants)
        {
            grant.IsActive = false;
            grant.UpdatedAt = DateTime.UtcNow;

            await _uow.StaffPermissions.UpdateAsync(
                grant, ct);
        }

        await _uow.SaveChangesAsync(ct);

        return true;
    }
}

// ================================================================
// GET STAFF PERMISSIONS
// ================================================================

public record GetStaffPermissionsQuery(Guid UserId)
    : IRequest<IEnumerable<StaffPermissionGrantDto>>;

public class GetStaffPermissionsQueryHandler
    : IRequestHandler<
        GetStaffPermissionsQuery,
        IEnumerable<StaffPermissionGrantDto>>
{
    private readonly IUnitOfWork _uow;

    public GetStaffPermissionsQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<StaffPermissionGrantDto>> Handle(
        GetStaffPermissionsQuery req,
        CancellationToken ct)
    {
        var user = await _uow.Users.GetByIdAsync(
            req.UserId, ct)
            ?? throw new NotFoundException("User", req.UserId);

        var grants = await _uow.StaffPermissions.GetByUserAsync(
            req.UserId, ct);

        return grants.Select(grant =>
            new StaffPermissionGrantDto(
                grant.Id,
                grant.UserId,
                user.FullName,
                grant.Permission.ToString(),
                (int)grant.Permission,
                grant.WarehouseId,
                grant.Warehouse?.Name,
                grant.IsActive,
                grant.GrantedAt));
    }
}

// ================================================================
// CHECK STAFF PERMISSION
// ================================================================

public record CheckPermissionQuery(
    Guid UserId,
    StaffPermission Permission,
    Guid? WarehouseId = null)
    : IRequest<bool>;

public class CheckPermissionQueryHandler
    : IRequestHandler<CheckPermissionQuery, bool>
{
    private readonly IPermissionService _permSvc;

    public CheckPermissionQueryHandler(IPermissionService permSvc)
    {
        _permSvc = permSvc;
    }

    public Task<bool> Handle(
        CheckPermissionQuery req,
        CancellationToken ct)
    {
        return _permSvc.HasStaffPermissionAsync(
            req.UserId,
            req.Permission,
            req.WarehouseId,
            ct);
    }
}