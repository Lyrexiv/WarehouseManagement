using FluentAssertions;
using FluentValidation.TestHelper;
using WarehouseManagement.Application.Features.Auth.Commands.Login;

using WarehouseManagement.Application.Features.Products;
using WarehouseManagement.Application.Features.Warehouses;
using Xunit;

namespace WarehouseManagement.Tests.Unit.Validation;

public class LoginValidationTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Login_EmptyUsername_HasValidationError()
    {
        var result = _validator.TestValidate(new LoginCommand("", "password"));
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Login_EmptyPassword_HasValidationError()
    {
        var result = _validator.TestValidate(new LoginCommand("user", ""));
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Login_ValidData_HasNoErrors()
    {
        var result = _validator.TestValidate(new LoginCommand("username", "password"));
        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class CreateProductValidationTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public void CreateProduct_EmptySKU_HasValidationError()
    {
        var cmd = new CreateProductCommand(new CreateProductDto("", "BC001", "Name", null, "PCS", 10m, 0, Guid.NewGuid()));
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor("Dto.SKU");
    }

    [Fact]
    public void CreateProduct_NegativePrice_HasValidationError()
    {
        var cmd = new CreateProductCommand(new CreateProductDto("SKU", "BC", "Name", null, "PCS", -1m, 0, Guid.NewGuid()));
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor("Dto.UnitPrice");
    }

    [Fact]
    public void CreateProduct_ValidData_HasNoErrors()
    {
        var cmd = new CreateProductCommand(new CreateProductDto("SKU001", "BC001", "Product Name", null, "PCS", 10m, 5, Guid.NewGuid()));
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class CreateWarehouseValidationTests
{
    private readonly CreateWarehouseCommandValidator _validator = new();

    [Fact]
    public void CreateWarehouse_EmptyCode_HasValidationError()
    {
        var cmd = new CreateWarehouseCommand(new CreateWarehouseDto("", "Name", "Addr", null, null, 1000, null));
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor("Dto.Code");
    }

    [Fact]
    public void CreateWarehouse_ZeroCapacity_HasValidationError()
    {
        var cmd = new CreateWarehouseCommand(new CreateWarehouseDto("WH01", "Name", "Addr", null, null, 0, null));
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor("Dto.Capacity");
    }

    [Fact]
    public void CreateWarehouse_ValidData_HasNoErrors()
    {
        var cmd = new CreateWarehouseCommand(new CreateWarehouseDto("WH001", "Warehouse", "123 Street", "HCM", "VN", 5000, null));
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
