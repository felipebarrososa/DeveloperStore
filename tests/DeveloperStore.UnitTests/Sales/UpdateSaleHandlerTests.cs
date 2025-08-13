using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeveloperStore.Api.Sales;
using DeveloperStore.Application.DTOs;
using DeveloperStore.Application.Sales;
using DeveloperStore.Domain.Entities;
using DeveloperStore.Infrastructure.ReadModel;
using DeveloperStore.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NSubstitute;
using Xunit;

namespace DeveloperStore.UnitTests.Sales;

public class UpdateSaleHandlerTests
{
    private static UpdateSaleHandler BuildSut(out DeveloperStore.Infrastructure.Data.DeveloperStoreDbContext db)
    {
        db = TestUtils.NewDb();
        var logger = Substitute.For<ILogger<UpdateSaleHandler>>();
        var mongoDb = Substitute.For<IMongoDatabase>();
        var coll = Substitute.For<IMongoCollection<SaleDoc>>();
        mongoDb.GetCollection<SaleDoc>("sales", Arg.Any<MongoCollectionSettings>()).Returns(coll);

        // overload com FilterDefinition
        coll.ReplaceOneAsync(
                Arg.Any<FilterDefinition<SaleDoc>>(),
                Arg.Any<SaleDoc>(),
                Arg.Any<ReplaceOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Substitute.For<ReplaceOneResult>()));

        var rm = new SalesReadModel(mongoDb);
        return new UpdateSaleHandler(db, logger, rm);
    }

    [Fact]
    public async Task Update_Replaces_Items_And_Recalculates_Total()
    {
        var sut = BuildSut(out var db);

        var sale = new Sale
        {
            Number = "S-3000",
            Date = DateOnly.FromDateTime(DateTime.Today),
            CustomerId = 1,
            CustomerName = "A",
            BranchId = 1,
            BranchName = "B"
        };
        sale.Items.Add(new SaleItem { ProductId = 1, ProductName = "P1", Quantity = 4, UnitPrice = 100m, DiscountPercent = 0.10m, Total = 360m });
        sale.Total = 360m;
        db.Sales.Add(sale);
        await db.SaveChangesAsync();

        var dto = new SaleUpdateDto("S-3000", sale.Date, 1, "A", 1, "B", new[] {
            new SaleItemIn(2, "P2", 10, 50m) // 10 * 50 * 0.8 = 400
        }, false);

        await sut.Handle(new UpdateSaleCommand(sale.Id, dto), CancellationToken.None);

        var updated = db.Sales.First(x => x.Id == sale.Id);
        updated.Total.Should().Be(400m);
        updated.Items.Should().HaveCount(1);
        updated.Items[0].ProductName.Should().Be("P2");
    }
}
