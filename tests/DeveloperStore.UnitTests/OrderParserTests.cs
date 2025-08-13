using System.Linq;
using Xunit;
using FluentAssertions;
using DeveloperStore.Application.Common;
using DeveloperStore.Domain.Entities;

namespace DeveloperStore.UnitTests;

public class OrderParserTests
{
    [Fact]
    public void ApplyOrder_SingleField_Ascending_Should_Order_Correctly()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 3, Title = "C", Price = 30m },
            new Product { Id = 1, Title = "A", Price = 10m },
            new Product { Id = 2, Title = "B", Price = 20m }
        }.AsQueryable();

        // Act
        var result = OrderParser.ApplyOrder(products, "id");

        // Assert
        result.Should().NotBeNull();
        var ordered = result.ToList();
        ordered[0].Id.Should().Be(1);
        ordered[1].Id.Should().Be(2);
        ordered[2].Id.Should().Be(3);
    }

    [Fact]
    public void ApplyOrder_SingleField_Descending_Should_Order_Correctly()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Title = "A", Price = 10m },
            new Product { Id = 2, Title = "B", Price = 20m },
            new Product { Id = 3, Title = "C", Price = 30m }
        }.AsQueryable();

        // Act
        var result = OrderParser.ApplyOrder(products, "id desc");

        // Assert
        result.Should().NotBeNull();
        var ordered = result.ToList();
        ordered[0].Id.Should().Be(3);
        ordered[1].Id.Should().Be(2);
        ordered[2].Id.Should().Be(1);
    }

    [Fact]
    public void ApplyOrder_MultipleFields_Should_Order_Correctly()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Title = "A", Price = 30m },
            new Product { Id = 2, Title = "A", Price = 20m },
            new Product { Id = 3, Title = "B", Price = 10m }
        }.AsQueryable();

        // Act
        var result = OrderParser.ApplyOrder(products, "title, price desc");

        // Assert
        result.Should().NotBeNull();
        var ordered = result.ToList();
        ordered[0].Title.Should().Be("A");
        ordered[0].Price.Should().Be(30m);
        ordered[1].Title.Should().Be("A");
        ordered[1].Price.Should().Be(20m);
        ordered[2].Title.Should().Be("B");
        ordered[2].Price.Should().Be(10m);
    }

    [Fact]
    public void ApplyOrder_WithSpaces_Should_Handle_Correctly()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 2, Title = "B", Price = 20m },
            new Product { Id = 1, Title = "A", Price = 10m }
        }.AsQueryable();

        // Act
        var result = OrderParser.ApplyOrder(products, "  title  ");

        // Assert
        result.Should().NotBeNull();
        var ordered = result.ToList();
        ordered[0].Title.Should().Be("A");
        ordered[1].Title.Should().Be("B");
    }

    [Fact]
    public void ApplyOrder_InvalidField_Should_Return_Original_Query()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Title = "A", Price = 10m }
        }.AsQueryable();

        // Act
        var result = OrderParser.ApplyOrder(products, "invalidField");

        // Assert
        result.Should().BeSameAs(products);
    }

    [Fact]
    public void ApplyOrder_EmptyString_Should_Return_Original_Query()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Title = "A", Price = 10m }
        }.AsQueryable();

        // Act
        var result = OrderParser.ApplyOrder(products, "");

        // Assert
        result.Should().BeSameAs(products);
    }

    [Fact]
    public void ApplyOrder_NullString_Should_Return_Original_Query()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Title = "A", Price = 10m }
        }.AsQueryable();

        // Act
        var result = OrderParser.ApplyOrder(products, null!);

        // Assert
        result.Should().BeSameAs(products);
    }

    [Fact]
    public void ApplyOrder_CaseInsensitive_Should_Work()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 2, Title = "B", Price = 20m },
            new Product { Id = 1, Title = "A", Price = 10m }
        }.AsQueryable();

        // Act
        var result = OrderParser.ApplyOrder(products, "TITLE");

        // Assert
        result.Should().NotBeNull();
        var ordered = result.ToList();
        ordered[0].Title.Should().Be("A");
        ordered[1].Title.Should().Be("B");
    }
}
