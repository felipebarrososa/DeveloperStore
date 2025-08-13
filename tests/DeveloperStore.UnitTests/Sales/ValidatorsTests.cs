using FluentValidation.TestHelper;
using DeveloperStore.Application.Sales;
using DeveloperStore.Application.DTOs;
using DeveloperStore.Domain.Enums;
using Xunit;

namespace DeveloperStore.UnitTests.Sales;

public class ValidatorsTests
{
    private readonly SaleCreateValidator _createValidator;
    private readonly SaleUpdateValidator _updateValidator;
    private readonly SaleItemInValidator _itemValidator;

    public ValidatorsTests()
    {
        _createValidator = new SaleCreateValidator();
        _updateValidator = new SaleUpdateValidator();
        _itemValidator = new SaleItemInValidator();
    }

    [Fact]
    public void SaleCreateValidator_Valid_Data_Should_Pass()
    {
        // Arrange
        var dto = new SaleCreateDto(
            "S-1001",
            DateOnly.FromDateTime(DateTime.Today),
            1, "Cliente Teste", 1, "Filial Teste",
            new[] { new SaleItemIn(1, "Produto Teste", 4, 100m) }
        );

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SaleCreateValidator_Empty_Number_Should_Fail()
    {
        // Arrange
        var dto = new SaleCreateDto(
            "",
            DateOnly.FromDateTime(DateTime.Today),
            1, "Cliente Teste", 1, "Filial Teste",
            new[] { new SaleItemIn(1, "Produto Teste", 4, 100m) }
        );

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Number);
    }

    [Fact]
    public void SaleCreateValidator_Invalid_CustomerId_Should_Fail()
    {
        // Arrange
        var dto = new SaleCreateDto(
            "S-1001",
            DateOnly.FromDateTime(DateTime.Today),
            0, "Cliente Teste", 1, "Filial Teste",
            new[] { new SaleItemIn(1, "Produto Teste", 4, 100m) }
        );

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public void SaleCreateValidator_Empty_CustomerName_Should_Fail()
    {
        // Arrange
        var dto = new SaleCreateDto(
            "S-1001",
            DateOnly.FromDateTime(DateTime.Today),
            1, "", 1, "Filial Teste",
            new[] { new SaleItemIn(1, "Produto Teste", 4, 100m) }
        );

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerName);
    }

    [Fact]
    public void SaleCreateValidator_Empty_Items_Should_Fail()
    {
        // Arrange
        var dto = new SaleCreateDto(
            "S-1001",
            DateOnly.FromDateTime(DateTime.Today),
            1, "Cliente Teste", 1, "Filial Teste",
            Array.Empty<SaleItemIn>()
        );

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Fact]
    public void SaleItemInValidator_Valid_Data_Should_Pass()
    {
        // Arrange
        var item = new SaleItemIn(1, "Produto Teste", 4, 100m);

        // Act
        var result = _itemValidator.TestValidate(item);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SaleItemInValidator_Invalid_ProductId_Should_Fail()
    {
        // Arrange
        var item = new SaleItemIn(0, "Produto Teste", 4, 100m);

        // Act
        var result = _itemValidator.TestValidate(item);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    [Fact]
    public void SaleItemInValidator_Empty_ProductName_Should_Fail()
    {
        // Arrange
        var item = new SaleItemIn(1, "", 4, 100m);

        // Act
        var result = _itemValidator.TestValidate(item);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductName);
    }

    [Fact]
    public void SaleItemInValidator_Invalid_Quantity_Should_Fail()
    {
        // Arrange
        var item = new SaleItemIn(1, "Produto Teste", 0, 100m);

        // Act
        var result = _itemValidator.TestValidate(item);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void SaleItemInValidator_Invalid_UnitPrice_Should_Fail()
    {
        // Arrange
        var item = new SaleItemIn(1, "Produto Teste", 4, 0m);

        // Act
        var result = _itemValidator.TestValidate(item);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UnitPrice);
    }

    [Fact]
    public void SaleUpdateValidator_Valid_Data_Should_Pass()
    {
        // Arrange
        var dto = new SaleUpdateDto(
            "S-1001",
            DateOnly.FromDateTime(DateTime.Today),
            1, "Cliente Teste", 1, "Filial Teste",
            new[] { new SaleItemIn(1, "Produto Teste", 4, 100m) },
            false
        );

        // Act
        var result = _updateValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SaleUpdateValidator_Empty_BranchName_Should_Fail()
    {
        // Arrange
        var dto = new SaleUpdateDto(
            "S-1001",
            DateOnly.FromDateTime(DateTime.Today),
            1, "Cliente Teste", 1, "",
            new[] { new SaleItemIn(1, "Produto Teste", 4, 100m) },
            false
        );

        // Act
        var result = _updateValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BranchName);
    }
}
