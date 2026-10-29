using FluentValidation;
using MediatR;

using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Domain.Enums;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.InventoryAudits;

// ============================================================
// DTOs
// ============================================================

public record InventoryAuditDetailDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSKU,
    int SystemQuantity,
    int ActualQuantity,
    int Discrepancy,
    string? Note);

public record InventoryAuditDto(
    Guid Id,
    string AuditNumber,
    Guid WarehouseId,
    string WarehouseName,
    DateTime AuditDate,
    string Status,
    string? Note,
    IEnumerable<InventoryAuditDetailDto> Details,
    DateTime CreatedAt,
    DateTime? CompletedAt);

public record CreateInventoryAuditDto(
    Guid WarehouseId,
    DateTime AuditDate,
    string? Note);

public record AddAuditDetailDto(
    Guid ProductId,
    int ActualQuantity,
    string? Note);


// ============================================================
// COMMANDS
// ============================================================

public record CreateInventoryAuditCommand(
    CreateInventoryAuditDto Dto)
    : IRequest<InventoryAuditDto>;

public record CompleteInventoryAuditCommand(
    Guid AuditId,
    IEnumerable<AddAuditDetailDto> ActualCounts)
    : IRequest<InventoryAuditDto>;


// ============================================================
// QUERIES
// ============================================================

public record GetAllInventoryAuditsQuery(
    Guid? WarehouseId)
    : IRequest<IEnumerable<InventoryAuditDto>>;

public record GetInventoryAuditByIdQuery(
    Guid Id)
    : IRequest<InventoryAuditDto>;


// ============================================================
// VALIDATOR
// ============================================================

public class CreateInventoryAuditCommandValidator
    : AbstractValidator<CreateInventoryAuditCommand>
{
    public CreateInventoryAuditCommandValidator()
    {
        RuleFor(x => x.Dto.WarehouseId)
            .NotEmpty()
            .WithMessage("WarehouseId không được để trống.");

        RuleFor(x => x.Dto.AuditDate)
            .NotEmpty()
            .WithMessage("Ngày kiểm kê không được để trống.");
    }
}


// ============================================================
// CREATE INVENTORY AUDIT
// ============================================================

public class CreateInventoryAuditCommandHandler
    : IRequestHandler<CreateInventoryAuditCommand, InventoryAuditDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IPermissionService _perm;
    private readonly ICurrentUserService _me;

    public CreateInventoryAuditCommandHandler(
        IUnitOfWork uow,
        IPermissionService perm,
        ICurrentUserService me)
    {
        _uow = uow;
        _perm = perm;
        _me = me;
    }

    public async Task<InventoryAuditDto> Handle(
        CreateInventoryAuditCommand req,
        CancellationToken ct)
    {
        // ====================================================
        // LẤY USER ID TỪ JWT
        // ====================================================

        var userId = _me.UserId;

        if (userId is null)
        {
            throw new UnauthorizedException(
                "User is not authenticated.");
        }

        // ====================================================
        // KIỂM TRA QUYỀN TRUY CẬP KHO
        // ====================================================

        if (!await _perm.CanAccessWarehouseAsync(
                userId.Value,
                req.Dto.WarehouseId,
                ct))
        {
            throw new ForbiddenException(
                "Bạn không có quyền tạo kiểm kê cho kho này.");
        }

        // ====================================================
        // KIỂM TRA QUYỀN CREATE AUDIT
        // ====================================================

        if (!await _perm.HasStaffPermissionAsync(
                userId.Value,
                StaffPermission.CreateAudit,
                req.Dto.WarehouseId,
                ct))
        {
            throw new ForbiddenException(
                "Bạn không có quyền tạo đợt kiểm kê.");
        }

        // ====================================================
        // KIỂM TRA WAREHOUSE
        // ====================================================

        var warehouse =
            await _uow.Warehouses.GetByIdAsync(
                req.Dto.WarehouseId,
                ct)
            ?? throw new NotFoundException(
                "Warehouse",
                req.Dto.WarehouseId);

        // ====================================================
        // TẠO AUDIT NUMBER
        // ====================================================

        var auditNumber =
            $"AUD-{DateTime.UtcNow:yyyyMMdd}-" +
            $"{Guid.NewGuid().ToString()[..8].ToUpper()}";

        // ====================================================
        // LẤY INVENTORY HIỆN TẠI
        // ====================================================

        var inventories =
            await _uow.Inventories.GetByWarehouseAsync(
                req.Dto.WarehouseId,
                ct);

        // ====================================================
        // TẠO INVENTORY AUDIT
        // ====================================================

        var audit = new InventoryAudit
        {
            AuditNumber = auditNumber,

            WarehouseId =
                req.Dto.WarehouseId,

            AuditDate =
                req.Dto.AuditDate,

            Note =
                req.Dto.Note,

            Status =
                InventoryAuditStatus.InProgress,

            CreatedBy =
                userId.Value.ToString()
        };

        // ====================================================
        // TẠO DETAIL TỪ INVENTORY HIỆN TẠI
        // ====================================================

        foreach (var inv in inventories)
        {
            audit.Details.Add(
                new InventoryAuditDetail
                {
                    ProductId =
                        inv.ProductId,

                    SystemQuantity =
                        inv.Quantity,

                    // Ban đầu Actual = System
                    ActualQuantity =
                        inv.Quantity
                });
        }

        // ====================================================
        // SAVE
        // ====================================================

        await _uow.InventoryAudits.AddAsync(
            audit,
            ct);

        await _uow.SaveChangesAsync(ct);

        // ====================================================
        // RETURN DTO
        // ====================================================

        return MapDto(
            audit,
            warehouse.Name);
    }

    private static InventoryAuditDto MapDto(
        InventoryAudit audit,
        string warehouseName)
    {
        return new InventoryAuditDto(
            audit.Id,
            audit.AuditNumber,
            audit.WarehouseId,
            warehouseName,
            audit.AuditDate,
            audit.Status.ToString(),
            audit.Note,

            audit.Details.Select(
                d => new InventoryAuditDetailDto(
                    d.Id,
                    d.ProductId,
                    d.Product?.Name ?? "",
                    d.Product?.SKU ?? "",
                    d.SystemQuantity,
                    d.ActualQuantity,
                    d.Discrepancy,
                    d.Note)),

            audit.CreatedAt,
            audit.CompletedAt);
    }
}


// ============================================================
// COMPLETE INVENTORY AUDIT
// ============================================================

public class CompleteInventoryAuditCommandHandler
    : IRequestHandler<
        CompleteInventoryAuditCommand,
        InventoryAuditDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IPermissionService _perm;
    private readonly ICurrentUserService _me;

    public CompleteInventoryAuditCommandHandler(
        IUnitOfWork uow,
        IPermissionService perm,
        ICurrentUserService me)
    {
        _uow = uow;
        _perm = perm;
        _me = me;
    }

    public async Task<InventoryAuditDto> Handle(
        CompleteInventoryAuditCommand req,
        CancellationToken ct)
    {
        // ====================================================
        // LẤY USER ID TỪ JWT
        // ====================================================

        var userId = _me.UserId;

        if (userId is null)
        {
            throw new UnauthorizedException(
                "User is not authenticated.");
        }

        // ====================================================
        // LẤY AUDIT
        // ====================================================

        var audit =
            await _uow.InventoryAudits
                .GetWithDetailsAsync(
                    req.AuditId,
                    ct)
            ?? throw new NotFoundException(
                "InventoryAudit",
                req.AuditId);

        // ====================================================
        // KIỂM TRA QUYỀN TRUY CẬP KHO
        // ====================================================

        if (!await _perm.CanAccessWarehouseAsync(
                userId.Value,
                audit.WarehouseId,
                ct))
        {
            throw new ForbiddenException(
                "Bạn không có quyền hoàn thành kiểm kê cho kho này.");
        }

        // ====================================================
        // KIỂM TRA QUYỀN COMPLETE AUDIT
        // ====================================================

        if (!await _perm.HasStaffPermissionAsync(
                userId.Value,
                StaffPermission.CompleteAudit,
                audit.WarehouseId,
                ct))
        {
            throw new ForbiddenException(
                "Bạn không có quyền hoàn thành đợt kiểm kê.");
        }

        // ====================================================
        // KIỂM TRA STATUS
        // ====================================================

        if (audit.Status != InventoryAuditStatus.InProgress)
        {
            throw new BusinessException(
                "Đợt kiểm kê không ở trạng thái đang thực hiện.");
        }

        // ====================================================
        // BEGIN TRANSACTION
        // ====================================================

        await _uow.BeginTransactionAsync(ct);

        try
        {
            // =================================================
            // UPDATE ACTUAL QUANTITY
            // =================================================

            foreach (var c in req.ActualCounts)
            {
                var detail =
                    audit.Details.FirstOrDefault(
                        x => x.ProductId == c.ProductId);

                if (detail != null)
                {
                    detail.ActualQuantity =
                        c.ActualQuantity;

                    detail.Note =
                        c.Note;
                }
            }

            // =================================================
            // UPDATE INVENTORY NẾU CÓ CHÊNH LỆCH
            // =================================================

            foreach (
                var detail in audit.Details
                    .Where(x => x.Discrepancy != 0))
            {
                var inventory =
                    await _uow.Inventories
                        .GetByWarehouseAndProductAsync(
                            audit.WarehouseId,
                            detail.ProductId,
                            ct);

                if (inventory != null)
                {
                    var quantityBefore =
                        inventory.Quantity;

                    // Điều chỉnh tồn kho về số thực tế
                    inventory.AdjustStock(
                        detail.ActualQuantity);

                    await _uow.Inventories.UpdateAsync(
                        inventory,
                        ct);

                    // =========================================
                    // TẠO STOCK MOVEMENT
                    // =========================================

                    await _uow.StockMovements.AddAsync(
                        new StockMovement
                        {
                            WarehouseId =
                                audit.WarehouseId,

                            ProductId =
                                detail.ProductId,

                            MovementType =
                                MovementType.AuditAdjustment,

                            Quantity =
                                detail.Discrepancy,

                            QuantityBefore =
                                quantityBefore,

                            QuantityAfter =
                                detail.ActualQuantity,

                            ReferenceNumber =
                                audit.AuditNumber,

                            CreatedByUserId =
                                userId.Value
                        },
                        ct);
                }
            }

            // =================================================
            // COMPLETE AUDIT
            // =================================================

            audit.Status =
                InventoryAuditStatus.Completed;

            audit.CompletedAt =
                DateTime.UtcNow;

            audit.CompletedByUserId =
                userId.Value;

            await _uow.InventoryAudits.UpdateAsync(
                audit,
                ct);

            // =================================================
            // SAVE
            // =================================================

            await _uow.SaveChangesAsync(ct);

            // =================================================
            // COMMIT
            // =================================================

            await _uow.CommitTransactionAsync(ct);

            // =================================================
            // RETURN DTO
            // =================================================

            return new InventoryAuditDto(
                audit.Id,
                audit.AuditNumber,
                audit.WarehouseId,
                audit.Warehouse?.Name ?? "",
                audit.AuditDate,
                audit.Status.ToString(),
                audit.Note,

                audit.Details.Select(
                    d => new InventoryAuditDetailDto(
                        d.Id,
                        d.ProductId,
                        d.Product?.Name ?? "",
                        d.Product?.SKU ?? "",
                        d.SystemQuantity,
                        d.ActualQuantity,
                        d.Discrepancy,
                        d.Note)),

                audit.CreatedAt,
                audit.CompletedAt);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }
}


// ============================================================
// GET ALL INVENTORY AUDITS
// ============================================================

public class GetAllInventoryAuditsQueryHandler
    : IRequestHandler<
        GetAllInventoryAuditsQuery,
        IEnumerable<InventoryAuditDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _me;
    private readonly IPermissionService _perm;

    public GetAllInventoryAuditsQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService me,
        IPermissionService perm)
    {
        _uow = uow;
        _me = me;
        _perm = perm;
    }

    public async Task<IEnumerable<InventoryAuditDto>> Handle(
        GetAllInventoryAuditsQuery req,
        CancellationToken ct)
    {
        // ====================================================
        // LẤY USER ID
        // ====================================================

        var userId = _me.UserId;

        if (userId is null)
        {
            throw new UnauthorizedException(
                "User is not authenticated.");
        }

        // ====================================================
        // LẤY TẤT CẢ AUDIT
        // ====================================================

        var all =
            await _uow.InventoryAudits.GetAllAsync(ct);

        // ====================================================
        // LỌC RECORD CHƯA BỊ DELETE
        // ====================================================

        var query =
            all.Where(a => !a.IsDeleted);

        // ====================================================
        // ADMIN
        // ====================================================

        if (!_me.IsAdmin)
        {
            // ================================================
            // MANAGER / STAFF
            // Chỉ lấy những warehouse được phép truy cập
            // ================================================

            var accessibleWarehouseIds =
                await _perm.GetAccessibleWarehouseIdsAsync(
                    userId.Value,
                    ct);

            if (accessibleWarehouseIds != null)
            {
                query =
                    query.Where(
                        a => accessibleWarehouseIds.Contains(
                            a.WarehouseId));
            }
        }

        // ====================================================
        // FILTER WAREHOUSE
        // ====================================================

        if (req.WarehouseId.HasValue)
        {
            query =
                query.Where(
                    a => a.WarehouseId ==
                         req.WarehouseId.Value);
        }

        // ====================================================
        // ORDER + MAP DTO
        // ====================================================

        return query
            .OrderByDescending(
                a => a.CreatedAt)

            .Select(
                a => new InventoryAuditDto(
                    a.Id,
                    a.AuditNumber,
                    a.WarehouseId,
                    a.Warehouse?.Name ?? "",
                    a.AuditDate,
                    a.Status.ToString(),
                    a.Note,

                    a.Details.Select(
                        d => new InventoryAuditDetailDto(
                            d.Id,
                            d.ProductId,
                            d.Product?.Name ?? "",
                            d.Product?.SKU ?? "",
                            d.SystemQuantity,
                            d.ActualQuantity,
                            d.Discrepancy,
                            d.Note)),

                    a.CreatedAt,
                    a.CompletedAt));
    }
}


// ============================================================
// GET INVENTORY AUDIT BY ID
// ============================================================

public class GetInventoryAuditByIdQueryHandler
    : IRequestHandler<
        GetInventoryAuditByIdQuery,
        InventoryAuditDto>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _me;
    private readonly IPermissionService _perm;

    public GetInventoryAuditByIdQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService me,
        IPermissionService perm)
    {
        _uow = uow;
        _me = me;
        _perm = perm;
    }

    public async Task<InventoryAuditDto> Handle(
        GetInventoryAuditByIdQuery req,
        CancellationToken ct)
    {
        // ====================================================
        // LẤY USER ID
        // ====================================================

        var userId = _me.UserId;

        if (userId is null)
        {
            throw new UnauthorizedException(
                "User is not authenticated.");
        }

        // ====================================================
        // LẤY AUDIT
        // ====================================================

        var audit =
            await _uow.InventoryAudits
                .GetWithDetailsAsync(
                    req.Id,
                    ct)
            ?? throw new NotFoundException(
                "InventoryAudit",
                req.Id);

        // ====================================================
        // CHECK WAREHOUSE ACCESS
        // ====================================================

        if (!await _perm.CanAccessWarehouseAsync(
                userId.Value,
                audit.WarehouseId,
                ct))
        {
            throw new ForbiddenException(
                "Bạn không có quyền xem kiểm kê này.");
        }

        // ====================================================
        // RETURN DTO
        // ====================================================

        return new InventoryAuditDto(
            audit.Id,
            audit.AuditNumber,
            audit.WarehouseId,
            audit.Warehouse?.Name ?? "",
            audit.AuditDate,
            audit.Status.ToString(),
            audit.Note,

            audit.Details.Select(
                d => new InventoryAuditDetailDto(
                    d.Id,
                    d.ProductId,
                    d.Product?.Name ?? "",
                    d.Product?.SKU ?? "",
                    d.SystemQuantity,
                    d.ActualQuantity,
                    d.Discrepancy,
                    d.Note)),

            audit.CreatedAt,
            audit.CompletedAt);
    }
}