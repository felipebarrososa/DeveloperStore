using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using DeveloperStore.Api.Sales;
using DeveloperStore.Application.DTOs;
using DeveloperStore.Application.Sales;
using DeveloperStore.Infrastructure.ReadModel;
using DeveloperStore.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NSubstitute;
using Xunit;

namespace DeveloperStore.UnitTests.Sales;

public class CreateSaleHandlerTests
{
    private static (CreateSaleHandler handler, IMongoCollection<SaleDoc> coll) BuildSut(out IMapper mapper)
    {
        var db = TestUtils.NewDb();
        mapper = TestUtils.NewMapper();
        var logger = Substitute.For<ILogger<CreateSaleHandler>>();

        var mongoDb = Substitute.For<IMongoDatabase>();
        var coll = Substitute.For<IMongoCollection<SaleDoc>>();
        mongoDb.GetCollection<SaleDoc>("sales", Arg.Any<MongoCollectionSettings>()).Returns(coll);

        
        coll.ReplaceOneAsync(
                Arg.Any<FilterDefinition<SaleDoc>>(),
                Arg.Any<SaleDoc>(),
                Arg.Any<ReplaceOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Substitute.For<ReplaceOneResult>()));

        var rm = new SalesReadModel(mongoDb);
        var handler = new CreateSaleHandler(db, mapper, logger, rm);
        return (handler, coll);
    }

    [Fact]
    public async Task Creates_Sale_With_10Percent_Discount_For_Qty4()
    {
        var (sut, coll) = BuildSut(out _);

        var dto = new SaleCreateDto(
            "S-1001",
            DateOnly.FromDateTime(DateTime.Today),
            1, "Cliente", 1, "Centro",
            new[] { new SaleItemIn(10, "Mouse", 4, 100m) }
        );

        var result = await sut.Handle(new CreateSaleCommand(dto), CancellationToken.None);

        result.Number.Should().Be("S-1001");
        result.Total.Should().Be(360m);

        await coll.Received(1).ReplaceOneAsync(
            Arg.Any<FilterDefinition<SaleDoc>>(),
            Arg.Any<SaleDoc>(),
            Arg.Any<ReplaceOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Creates_Sale_With_20Percent_Discount_For_Qty10()
    {
        var (sut, _) = BuildSut(out _);

        var dto = new SaleCreateDto(
            "S-1002",
            DateOnly.FromDateTime(DateTime.Today),
            1, "Cliente", 1, "Centro",
            new[] { new SaleItemIn(10, "Teclado", 10, 50m) }
        );

        var result = await sut.Handle(new CreateSaleCommand(dto), CancellationToken.None);
        result.Total.Should().Be(400m); // 10 * 50 * 0.8
    }

    [Fact]
    public async Task Throws_When_Qty_Above_20()
    {
        var (sut, _) = BuildSut(out _);

        var dto = new SaleCreateDto(
            "S-1003",
            DateOnly.FromDateTime(DateTime.Today),
            1, "Cliente", 1, "Centro",
            new[] { new SaleItemIn(10, "Monitor", 21, 100m) }
        );

        var act = async () => await sut.Handle(new CreateSaleCommand(dto), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Throws_When_Duplicate_Number()
    {
        var (sut, _) = BuildSut(out _);

        var dto1 = new SaleCreateDto("S-2000", DateOnly.FromDateTime(DateTime.Today), 1, "A", 1, "B",
            new[] { new SaleItemIn(1, "Item", 4, 10m) });
        _ = await sut.Handle(new CreateSaleCommand(dto1), CancellationToken.None);

        var dto2 = new SaleCreateDto("S-2000", DateOnly.FromDateTime(DateTime.Today), 1, "A", 1, "B",
            new[] { new SaleItemIn(1, "Item", 4, 10m) });

        var act = async () => await sut.Handle(new CreateSaleCommand(dto2), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }
}
