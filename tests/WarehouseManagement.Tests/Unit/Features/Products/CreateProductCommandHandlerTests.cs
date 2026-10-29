using FluentAssertions;
using Moq;
using WarehouseManagement.Application.Features.Products;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;
using Xunit;

namespace WarehouseManagement.Tests.Unit.Features.Products;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IProductRepository> _productRepo;
    private readonly Mock<ICategoryRepository> _categoryRepo;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _uow = new Mock<IUnitOfWork>();
        _productRepo = new Mock<IProductRepository>();
        _categoryRepo = new Mock<ICategoryRepository>();

        _uow.Setup(u => u.Products).Returns(_productRepo.Object);
        _uow.Setup(u => u.Categories).Returns(_categoryRepo.Object);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _handler = new CreateProductCommandHandler(_uow.Object);
    }

    [Fact]
    public async Task Handle_ValidProduct_ReturnsProductDto()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, Name = "Electronics" };

        _productRepo.Setup(r => r.SkuExistsAsync("SKU001", null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _productRepo.Setup(r => r.BarcodeExistsAsync("BC001", null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _categoryRepo.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _productRepo.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, CancellationToken _) => p);

        var command = new CreateProductCommand(new CreateProductDto(
            "SKU001", "BC001", "Test Product", "Description",
            "PCS", 99.99m, 5, categoryId));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SKU.Should().Be("SKU001");
        result.Name.Should().Be("Test Product");
        result.CategoryName.Should().Be("Electronics");
        result.UnitPrice.Should().Be(99.99m);
    }

    [Fact]
    public async Task Handle_DuplicateSKU_ThrowsDuplicateCodeException()
    {
        _productRepo.Setup(r => r.SkuExistsAsync("SKU001", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var command = new CreateProductCommand(new CreateProductDto(
            "SKU001", "BC001", "Test", null, "PCS", 10m, 0, Guid.NewGuid()));

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateCodeException>();
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ThrowsNotFoundException()
    {
        var categoryId = Guid.NewGuid();
        _productRepo.Setup(r => r.SkuExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _productRepo.Setup(r => r.BarcodeExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _categoryRepo.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        var command = new CreateProductCommand(new CreateProductDto(
            "SKU001", "BC001", "Test", null, "PCS", 10m, 0, categoryId));

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
