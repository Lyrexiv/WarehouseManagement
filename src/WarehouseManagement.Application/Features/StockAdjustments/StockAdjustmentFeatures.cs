using FluentValidation;
using MediatR;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Domain.Enums;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.StockAdjustments;

// ================================================================
// DTOs
// ================================================================

public record StockAdjustmentDto(
    Guid Id,
    string AdjustmentNumber,
    Guid WarehouseId,
    string WarehouseName,
    Guid ProductId,
    string ProductName,
    int QuantityBefore,
    int QuantityAfter,
    int AdjustmentQuantity,
    string AdjustmentType,
    string Reason,
    string? Note,
    DateTime CreatedAt);

public record CreateStockAdjustmentDto(
    Guid WarehouseId,
    Guid ProductId,
    int NewQuantity,
    string Reason,
    string? Note);

// ================================================================
// COMMANDS
// ================================================================

public record CreateStockAdjustmentCommand(
    CreateStockAdjustmentDto Dto,
    Guid CreatedByUserId) : IRequest<StockAdjustmentDto>;

// ================================================================
// QUERIES
// ================================================================

/// <summary>
/// Lay danh sach tat ca dieu chinh ton kho.
/// Co the loc theo WarehouseId hoac ProductId.
/// </summary>
public record GetAllStockAdjustmentsQuery(
    Guid? WarehouseId,
    Guid? ProductId) : IRequest<IEnumerable<StockAdjustmentDto>>;

// ================================================================
// VALIDATORS
// ================================================================

public class CreateStockAdjustmentCommandValidator : AbstractValidator<CreateStockAdjustmentCommand>
{
    public CreateStockAdjustmentCommandValidator()
    {
        RuleFor(x => x.Dto.WarehouseId)
            .NotEmpty().WithMessage("Kho không được để trống");

        RuleFor(x => x.Dto.ProductId)
            .NotEmpty().WithMessage("Sản phẩm không được để trống");

        RuleFor(x => x.Dto.NewQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Số lượng không được âm");

        RuleFor(x => x.Dto.Reason)
            .NotEmpty().WithMessage("Lý do điều chỉnh không được để trống")
            .MaximumLength(500);
    }
}

// ================================================================
// HANDLERS
// ================================================================

public class CreateStockAdjustmentCommandHandler
    : IRequestHandler<CreateStockAdjustmentCommand, StockAdjustmentDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateStockAdjustmentCommandHandler(IUnitOfWork unitOfWork)
        => _unitOfWork = unitOfWork;

    public async Task<StockAdjustmentDto> Handle(
        CreateStockAdjustmentCommand request,
        CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var dto = request.Dto;

            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId, cancellationToken)
                ?? throw new NotFoundException("Warehouse", dto.WarehouseId);

            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId, cancellationToken)
                ?? throw new NotFoundException("Product", dto.ProductId);

            // Lay hoac tao ban ghi ton kho
            var inventory = await _unitOfWork.Inventories
                .GetByWarehouseAndProductAsync(dto.WarehouseId, dto.ProductId, cancellationToken);

            var qtyBefore = inventory?.Quantity ?? 0;

            if (inventory == null)
            {
                inventory = new Inventory
                {
                    WarehouseId = dto.WarehouseId,
                    ProductId = dto.ProductId,
                    Quantity = 0
                };
                await _unitOfWork.Inventories.AddAsync(inventory, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // Cap nhat so luong
            inventory.AdjustStock(dto.NewQuantity);
            await _unitOfWork.Inventories.UpdateAsync(inventory, cancellationToken);

            // Xac dinh loai dieu chinh
            var adjType = dto.NewQuantity >= qtyBefore
                ? StockAdjustmentType.Increase
                : StockAdjustmentType.Decrease;

            var adjNumber = $"ADJ-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

            // Tao phieu dieu chinh
            var adjustment = new StockAdjustment
            {
                AdjustmentNumber = adjNumber,
                WarehouseId = dto.WarehouseId,
                ProductId = dto.ProductId,
                QuantityBefore = qtyBefore,
                QuantityAfter = dto.NewQuantity,
                AdjustmentType = adjType,
                Reason = dto.Reason,
                Note = dto.Note,
                CreatedBy = request.CreatedByUserId.ToString()
            };
            await _unitOfWork.StockAdjustments.AddAsync(adjustment, cancellationToken);

            // Ghi lich su bien dong
            var movement = new StockMovement
            {
                WarehouseId = dto.WarehouseId,
                ProductId = dto.ProductId,
                MovementType = MovementType.Adjustment,
                Quantity = dto.NewQuantity - qtyBefore,
                QuantityBefore = qtyBefore,
                QuantityAfter = dto.NewQuantity,
                ReferenceNumber = adjNumber,
                Note = dto.Reason,
                CreatedByUserId = request.CreatedByUserId
            };
            await _unitOfWork.StockMovements.AddAsync(movement, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return new StockAdjustmentDto(
                adjustment.Id,
                adjNumber,
                dto.WarehouseId,
                warehouse.Name,
                dto.ProductId,
                product.Name,
                qtyBefore,
                dto.NewQuantity,
                dto.NewQuantity - qtyBefore,
                adjType.ToString(),
                dto.Reason,
                dto.Note,
                adjustment.CreatedAt);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

// ----------------------------------------------------------------
// GET ALL HANDLER
// ----------------------------------------------------------------

public class GetAllStockAdjustmentsQueryHandler
    : IRequestHandler<GetAllStockAdjustmentsQuery, IEnumerable<StockAdjustmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllStockAdjustmentsQueryHandler(IUnitOfWork unitOfWork)
        => _unitOfWork = unitOfWork;

    public async Task<IEnumerable<StockAdjustmentDto>> Handle(
        GetAllStockAdjustmentsQuery request,
        CancellationToken cancellationToken)
    {
        // Lấy tất cả (chưa có filter trong repo — lọc ở đây)
        var all = await _unitOfWork.StockAdjustments
            .GetAllAsync(cancellationToken);

        // Không dùng AsQueryable()
        var query = all.Where(a => !a.IsDeleted);

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(
                a => a.WarehouseId == request.WarehouseId.Value);
        }

        if (request.ProductId.HasValue)
        {
            query = query.Where(
                a => a.ProductId == request.ProductId.Value);
        }

        return query
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new StockAdjustmentDto(
                a.Id,
                a.AdjustmentNumber,
                a.WarehouseId,
                a.Warehouse?.Name ?? string.Empty,

                a.ProductId,
                a.Product?.Name ?? string.Empty,

                a.QuantityBefore,
                a.QuantityAfter,
                a.AdjustmentQuantity,

                a.AdjustmentType.ToString(),
                a.Reason,
                a.Note,
                a.CreatedAt
            ))
            .ToList();
    }
}