using Xunit;
using FluentAssertions;
using DeveloperStore.Application.Sales;

namespace DeveloperStore.UnitTests.Sales;

public class DiscountCalculatorTests
{
    [Theory]
    [InlineData(1, 0.00)]
    [InlineData(3, 0.00)]
    [InlineData(4, 0.10)]
    [InlineData(9, 0.10)]
    [InlineData(10, 0.20)]
    [InlineData(20, 0.20)]
    public void Discount_By_Quantity(int qty, double expected)
    {
        var (d, err) = DiscountCalculator.FromQuantity(qty);
        d.Should().Be((decimal)expected);
        err.Should().BeNull();
    }

    [Fact]
    public void Discount_Above_20_Should_Error()
    {
        var (d, err) = DiscountCalculator.FromQuantity(21);
        d.Should().Be(0m);
        err.Should().NotBeNull();
    }
}
