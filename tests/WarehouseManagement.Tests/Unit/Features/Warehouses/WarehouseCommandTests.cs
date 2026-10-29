using FluentAssertions;
using Moq;
using WarehouseManagement.Application.Features.Warehouses;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;
using Xunit;

namespace WarehouseManagement.Tests.Unit.Features.Warehouses;

public class WarehouseCommandTests
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IWarehouseRepository> _warehouseRepo;

    public WarehouseCommandTests()
    {
        _uow = new Mock<IUnitOfWork>();
        _warehouseRepo = new Mock<IWarehouseRepository>();
        _uow.Setup(u => u.Warehouses).Returns(_warehouseRepo.Object);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task CreateWarehouse_ValidData_ReturnsWarehouseDto()
    {
        _warehouseRepo.Setup(r => r.CodeExistsAsync("WH001", null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _warehouseRepo.Setup(r => r.AddAsync(It.IsAny<Warehouse>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Warehouse w, CancellationToken _) => w);

        var handler = new CreateWarehouseCommandHandler(_uow.Object);
        var cmd = new CreateWarehouseCommand(new CreateWarehouseDto("WH001", "Main Warehouse", "123 Street", "HCMC", "VN", 5000, null));

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().Be("WH001");
        result.Name.Should().Be("Main Warehouse");
        result.Capacity.Should().Be(5000);
    }

    [Fact]
    public async Task CreateWarehouse_DuplicateCode_ThrowsDuplicateCodeException()
    {
        _warehouseRepo.Setup(r => r.CodeExistsAsync("WH001", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var handler = new CreateWarehouseCommandHandler(_uow.Object);
        var cmd = new CreateWarehouseCommand(new CreateWarehouseDto("WH001", "Test", "Addr", null, null, 1000, null));

        var act = async () => await handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateCodeException>().WithMessage("*WH001*");
    }

    [Fact]
    public async Task DeleteWarehouse_ExistingWarehouse_ReturnsTrue()
    {
        var warehouse = new Warehouse { Id = Guid.NewGuid(), Code = "WH001", Name = "Test" };
        _warehouseRepo.Setup(r => r.GetByIdAsync(warehouse.Id, It.IsAny<CancellationToken>())).ReturnsAsync(warehouse);
        _warehouseRepo.Setup(r => r.UpdateAsync(It.IsAny<Warehouse>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new DeleteWarehouseCommandHandler(_uow.Object);
        var result = await handler.Handle(new DeleteWarehouseCommand(warehouse.Id), CancellationToken.None);

        result.Should().BeTrue();
        warehouse.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteWarehouse_NotFound_ThrowsNotFoundException()
    {
        _warehouseRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Warehouse?)null);

        var handler = new DeleteWarehouseCommandHandler(_uow.Object);
        var act = async () => await handler.Handle(new DeleteWarehouseCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
