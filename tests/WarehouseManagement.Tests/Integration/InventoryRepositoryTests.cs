using FluentAssertions;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Infrastructure.Persistence;
using WarehouseManagement.Infrastructure.Persistence.Repositories;
using Xunit;

namespace WarehouseManagement.Tests.Integration;

public class InventoryRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _ctx;
    private readonly InventoryRepository _repo;
    private readonly Warehouse _warehouse;
    private readonly Product _product;
    private readonly Category _category;

    public InventoryRepositoryTests()
    {
        _ctx  = InMemoryDbContextFactory.Create();
        _repo = new InventoryRepository(_ctx);

        _category  = new Category { Name = "Test Category" };
        _warehouse = new Warehouse { Code = "WH001", Name = "Test Warehouse", Address = "Test", Capacity = 1000 };
        _product   = new Product { SKU = "SKU001", Barcode = "BC001", Name = "Test Product", Unit = "PCS", UnitPrice = 100, MinimumStock = 10, Category = _category };

        _ctx.Categories.Add(_category);
        _ctx.Warehouses.Add(_warehouse);
        _ctx.Products.Add(_product);
        _ctx.SaveChanges();
    }

    [Fact]
    public async Task AddInventory_ThenGetByWarehouseAndProduct_ReturnsCorrectItem()
    {
        var inv = new Inventory { WarehouseId = _warehouse.Id, ProductId = _product.Id, Quantity = 100 };
        await _repo.AddAsync(inv);
        await _ctx.SaveChangesAsync();

        var result = await _repo.GetByWarehouseAndProductAsync(_warehouse.Id, _product.Id);

        result.Should().NotBeNull();
        result!.Quantity.Should().Be(100);
    }

    [Fact]
    public async Task GetByWarehouseAsync_ReturnsAllItemsInWarehouse()
    {
        var product2 = new Product { SKU = "SKU002", Barcode = "BC002", Name = "P2", Unit = "BOX", UnitPrice = 50, Category = _category };
        _ctx.Products.Add(product2);
        await _ctx.SaveChangesAsync();

        await _repo.AddAsync(new Inventory { WarehouseId = _warehouse.Id, ProductId = _product.Id, Quantity = 50 });
        await _repo.AddAsync(new Inventory { WarehouseId = _warehouse.Id, ProductId = product2.Id, Quantity = 30 });
        await _ctx.SaveChangesAsync();

        var result = await _repo.GetByWarehouseAsync(_warehouse.Id);

        result.Should().HaveCount(2);
        result.Sum(i => i.Quantity).Should().Be(80);
    }

    [Fact]
    public async Task GetLowStockAsync_ReturnsOnlyLowStockItems()
    {
        // qty 5 < minStock 10 => low stock
        await _repo.AddAsync(new Inventory { WarehouseId = _warehouse.Id, ProductId = _product.Id, Quantity = 5 });
        await _ctx.SaveChangesAsync();

        var result = await _repo.GetLowStockAsync();

        result.Should().HaveCount(1);
        result[0].Quantity.Should().Be(5);
    }

    [Fact]
    public async Task GetTotalInventoryValueAsync_ReturnsSumOfAllValues()
    {
        await _repo.AddAsync(new Inventory { WarehouseId = _warehouse.Id, ProductId = _product.Id, Quantity = 10 });
        await _ctx.SaveChangesAsync();

        var totalValue = await _repo.GetTotalInventoryValueAsync();

        // 10 * 100 = 1000
        totalValue.Should().Be(1000);
    }

    public void Dispose() => _ctx.Dispose();
}
