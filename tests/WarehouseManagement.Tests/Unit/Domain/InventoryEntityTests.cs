using FluentAssertions;
using WarehouseManagement.Domain.Entities;
using Xunit;

namespace WarehouseManagement.Tests.Unit.Domain;

public class InventoryEntityTests
{
    private static Inventory CreateInventory(int quantity = 100, int minStock = 10)
    {
        var product = new Product { Name = "Test Product", MinimumStock = minStock, UnitPrice = 50 };
        return new Inventory { Product = product, Quantity = quantity, ReservedQuantity = 0 };
    }

    [Fact]
    public void AddStock_ValidQuantity_IncreasesStock()
    {
        var inv = CreateInventory(100);
        inv.AddStock(50);
        inv.Quantity.Should().Be(150);
    }

    [Fact]
    public void AddStock_ZeroQuantity_ThrowsArgumentException()
    {
        var inv = CreateInventory(100);
        var act = () => inv.AddStock(0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddStock_NegativeQuantity_ThrowsArgumentException()
    {
        var inv = CreateInventory(100);
        var act = () => inv.AddStock(-5);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RemoveStock_ValidQuantity_DecreasesStock()
    {
        var inv = CreateInventory(100);
        inv.RemoveStock(30);
        inv.Quantity.Should().Be(70);
    }

    [Fact]
    public void RemoveStock_InsufficientStock_ThrowsInvalidOperationException()
    {
        var inv = CreateInventory(10);
        var act = () => inv.RemoveStock(50);
        act.Should().Throw<InvalidOperationException>().WithMessage("*Insufficient*");
    }

    [Fact]
    public void RemoveStock_ExactAmount_ResultsInZeroStock()
    {
        var inv = CreateInventory(50);
        inv.RemoveStock(50);
        inv.Quantity.Should().Be(0);
    }

    [Fact]
    public void AdjustStock_ValidQuantity_SetsNewQuantity()
    {
        var inv = CreateInventory(100);
        inv.AdjustStock(75);
        inv.Quantity.Should().Be(75);
    }

    [Fact]
    public void AdjustStock_NegativeQuantity_ThrowsArgumentException()
    {
        var inv = CreateInventory(100);
        var act = () => inv.AdjustStock(-1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AvailableQuantity_WithReservation_ReturnsCorrectValue()
    {
        var inv = CreateInventory(100);
        inv.ReservedQuantity = 30;
        inv.AvailableQuantity.Should().Be(70);
    }

    [Theory]
    [InlineData(10, 10, true)]
    [InlineData(9, 10, true)]
    [InlineData(11, 10, false)]
    [InlineData(0, 10, true)]
    public void IsLowStock_VariousQuantities_ReturnsExpected(int qty, int minStock, bool expected)
    {
        var inv = CreateInventory(qty, minStock);
        inv.IsLowStock.Should().Be(expected);
    }
}
