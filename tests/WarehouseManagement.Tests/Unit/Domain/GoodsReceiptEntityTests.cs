using FluentAssertions;
using WarehouseManagement.Domain.Entities;
using Xunit;

namespace WarehouseManagement.Tests.Unit.Domain;

public class GoodsReceiptEntityTests
{
    [Fact]
    public void CalculateTotalAmount_WithDetails_ReturnsSumOfAllDetails()
    {
        var receipt = new GoodsReceipt
        {
            Details = new List<GoodsReceiptDetail>
            {
                new() { Quantity = 10, UnitPrice = 100 },
                new() { Quantity = 5,  UnitPrice = 200 },
                new() { Quantity = 20, UnitPrice = 50  }
            }
        };

        receipt.CalculateTotalAmount();

        receipt.TotalAmount.Should().Be(3000); // 1000 + 1000 + 1000
    }

    [Fact]
    public void CalculateTotalAmount_EmptyDetails_ReturnsZero()
    {
        var receipt = new GoodsReceipt { Details = new List<GoodsReceiptDetail>() };
        receipt.CalculateTotalAmount();
        receipt.TotalAmount.Should().Be(0);
    }

    [Fact]
    public void GoodsReceiptDetail_TotalPrice_IsCalculatedCorrectly()
    {
        var detail = new GoodsReceiptDetail { Quantity = 7, UnitPrice = 150 };
        detail.TotalPrice.Should().Be(1050);
    }
}
