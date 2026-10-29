using FluentAssertions;
using Moq;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.Inventories;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Domain.Interfaces;
using Xunit;

namespace WarehouseManagement.Tests.Unit.Features.Inventories;

public class InventoryQueryTests
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IInventoryRepository> _invRepo;
    private readonly Mock<ICurrentUserService> _currentUser;
    private readonly Mock<IPermissionService> _permissionService;

    public InventoryQueryTests()
    {
        _uow = new Mock<IUnitOfWork>();
        _invRepo = new Mock<IInventoryRepository>();

        _currentUser = new Mock<ICurrentUserService>();
        _permissionService = new Mock<IPermissionService>();

        _uow
            .Setup(u => u.Inventories)
            .Returns(_invRepo.Object);
    }

    [Fact]
    public async Task GetInventoryByWarehouse_ReturnsCorrectItems()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();

        var warehouse = new Warehouse
        {
            Id = warehouseId,
            Name = "WH1"
        };

        var product1 = new Product
        {
            Id = Guid.NewGuid(),
            Name = "P1",
            SKU = "S1",
            Unit = "PCS",
            UnitPrice = 100,
            MinimumStock = 10
        };

        var product2 = new Product
        {
            Id = Guid.NewGuid(),
            Name = "P2",
            SKU = "S2",
            Unit = "BOX",
            UnitPrice = 200,
            MinimumStock = 5
        };

        var inventories = new List<Inventory>
        {
            new()
            {
                Id = Guid.NewGuid(),
                WarehouseId = warehouseId,
                Warehouse = warehouse,
                Product = product1,
                ProductId = product1.Id,
                Quantity = 50
            },
            new()
            {
                Id = Guid.NewGuid(),
                WarehouseId = warehouseId,
                Warehouse = warehouse,
                Product = product2,
                ProductId = product2.Id,
                Quantity = 3
            }
        };

        _invRepo
            .Setup(r => r.GetByWarehouseAsync(
                warehouseId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventories);

        // Nếu Handler kiểm tra quyền truy cập warehouse,
        // cho phép test user truy cập warehouse này.
        SetupWarehouseAccess(warehouseId);

        var handler = new GetInventoryByWarehouseQueryHandler(
            _uow.Object,
            _currentUser.Object,
            _permissionService.Object);

        // Act
        var result = (
            await handler.Handle(
                new GetInventoryByWarehouseQuery(warehouseId),
                CancellationToken.None))
            .ToList();

        // Assert
        result.Should().HaveCount(2);

        result
            .First(r => r.ProductSKU == "S1")
            .Quantity
            .Should()
            .Be(50);

        result
            .First(r => r.ProductSKU == "S1")
            .IsLowStock
            .Should()
            .BeFalse();

        result
            .First(r => r.ProductSKU == "S2")
            .IsLowStock
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task GetLowStock_ReturnsOnlyLowStockItems()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Low Stock",
            SKU = "LS1",
            Unit = "PCS",
            UnitPrice = 50,
            MinimumStock = 20
        };

        var warehouse = new Warehouse
        {
            Id = warehouseId,
            Name = "WH1"
        };

        var inventories = new List<Inventory>
        {
            new()
            {
                Id = Guid.NewGuid(),
                WarehouseId = warehouseId,
                ProductId = product.Id,
                Product = product,
                Warehouse = warehouse,
                Quantity = 5
            }
        };

        _invRepo
            .Setup(r => r.GetLowStockAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventories);

        SetupWarehouseAccess(warehouseId);

        var handler = new GetLowStockQueryHandler(
            _uow.Object,
            _currentUser.Object,
            _permissionService.Object);

        // Act
        var result = (
            await handler.Handle(
                new GetLowStockQuery(),
                CancellationToken.None))
            .ToList();

        // Assert
        result.Should().HaveCount(1);

        result[0].IsLowStock.Should().BeTrue();
        result[0].Quantity.Should().Be(5);
        result[0].MinimumStock.Should().Be(20);
    }

    private void SetupWarehouseAccess(Guid warehouseId)
    {
        // Admin có thể truy cập tất cả warehouse.
        _currentUser
            .SetupGet(x => x.IsAdmin)
            .Returns(true);
    }
}