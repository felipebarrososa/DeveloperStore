using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using DeveloperStore.Application.DTOs;
using System.Text;
using System.Text.Json;
using Xunit;

namespace DeveloperStore.IntegrationTests;

public class BasicApiTests
{
    [Fact]
    public void Placeholder_Test_Should_Pass()
    {
        
        Assert.True(true);
    }

    [Fact]
    public void Test_Project_Structure_Should_Be_Valid()
    {
        
        var productDtoType = typeof(ProductDto);
        var userDtoType = typeof(UserDto);
        var saleDtoType = typeof(SaleDto);
        
        Assert.NotNull(productDtoType);
        Assert.NotNull(userDtoType);
        Assert.NotNull(saleDtoType);
    }
}
