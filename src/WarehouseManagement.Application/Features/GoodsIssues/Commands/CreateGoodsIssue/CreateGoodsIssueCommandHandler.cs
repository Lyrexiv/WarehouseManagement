using MediatR;

using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.GoodsIssues.DTOs;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Domain.Enums;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.GoodsIssues.Commands.CreateGoodsIssue;

public class CreateGoodsIssueCommandHandler
    : IRequestHandler<CreateGoodsIssueCommand, GoodsIssueDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IPermissionService _perm;

    public CreateGoodsIssueCommandHandler(
        IUnitOfWork uow,
        IPermissionService perm)
    {
        _uow = uow;
        _perm = perm;
    }

    public async Task<GoodsIssueDto> Handle(
        CreateGoodsIssueCommand req,
        CancellationToken ct)
    {
        // ----------------------------------------------------
        // Check quyền truy cập kho
        // ----------------------------------------------------

        if (!await _perm.CanAccessWarehouseAsync(
                req.CreatedByUserId,
                req.Dto.WarehouseId,
                ct))
        {
            throw new ForbiddenException(
                "Bạn không có quyền quản lý kho này.");
        }

        // ----------------------------------------------------
        // Check permission tạo phiếu xuất
        // ----------------------------------------------------

        if (!await _perm.HasStaffPermissionAsync(
                req.CreatedByUserId,
                StaffPermission.CreateGoodsIssue,
                req.Dto.WarehouseId,
                ct))
        {
            throw new ForbiddenException(
                "Bạn không có quyền tạo phiếu xuất kho.");
        }

        // ----------------------------------------------------
        // Kiểm tra Warehouse
        // ----------------------------------------------------

        var warehouse =
            await _uow.Warehouses.GetByIdAsync(
                req.Dto.WarehouseId,
                ct)
            ?? throw new NotFoundException(
                "Warehouse",
                req.Dto.WarehouseId);

        // ----------------------------------------------------
        // Generate Issue Number
        // ----------------------------------------------------

        var issueNumber =
            $"ISS-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        // ----------------------------------------------------
        // Create GoodsIssue
        // ----------------------------------------------------

        var issue = new GoodsIssue
        {
            IssueNumber = issueNumber,
            WarehouseId = req.Dto.WarehouseId,
            ExportDate = req.Dto.ExportDate,
            ReceiverName = req.Dto.ReceiverName,
            ReceiverDepartment = req.Dto.ReceiverDepartment,
            Purpose = req.Dto.Purpose,
            Note = req.Dto.Note,

            Status = GoodsIssueStatus.Draft,

            CreatedBy = req.CreatedByUserId.ToString()
        };

        // ----------------------------------------------------
        // Details
        // ----------------------------------------------------

        var detailDtos =
            new List<GoodsIssueDetailDto>();

        foreach (var d in req.Dto.Details)
        {
            // -----------------------------------------------
            // Product
            // -----------------------------------------------

            var product =
                await _uow.Products.GetByIdAsync(
                    d.ProductId,
                    ct)
                ?? throw new NotFoundException(
                    "Product",
                    d.ProductId);

            // -----------------------------------------------
            // Inventory
            // -----------------------------------------------

            var inv =
                await _uow.Inventories
                    .GetByWarehouseAndProductAsync(
                        req.Dto.WarehouseId,
                        d.ProductId,
                        ct);

            if (inv == null ||
                inv.AvailableQuantity < d.Quantity)
            {
                throw new InsufficientStockException(
                    product.Name,
                    d.Quantity,
                    inv?.AvailableQuantity ?? 0);
            }

            // -----------------------------------------------
            // Add detail
            // -----------------------------------------------

            issue.Details.Add(
                new GoodsIssueDetail
                {
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    Note = d.Note
                });

            // -----------------------------------------------
            // DTO
            // -----------------------------------------------

            detailDtos.Add(
                new GoodsIssueDetailDto(
                    Guid.Empty,
                    d.ProductId,
                    product.Name,
                    product.SKU,
                    d.Quantity,
                    d.UnitPrice,
                    d.Quantity * d.UnitPrice,
                    d.Note));
        }

        // ----------------------------------------------------
        // Save
        // ----------------------------------------------------

        await _uow.GoodsIssues.AddAsync(
            issue,
            ct);

        await _uow.SaveChangesAsync(ct);

        // ----------------------------------------------------
        // Return
        // ----------------------------------------------------

        return new GoodsIssueDto(
            issue.Id,
            issue.IssueNumber,
            issue.WarehouseId,
            warehouse.Name,
            issue.ExportDate,
            issue.ReceiverName,
            issue.ReceiverDepartment,
            issue.Purpose,
            issue.Status.ToString(),
            issue.Note,
            detailDtos,
            issue.CreatedAt);
    }
}