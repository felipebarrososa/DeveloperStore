using System;
using System.Threading;
using System.Threading.Tasks;
using DeveloperStore.Api.Sales;
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

public class CancelHandlersTests
{
    private static (CancelSaleHandler cancelSale, CancelSaleItemHandler cancelItem, DeveloperStore.Infrastructure.Data.DeveloperStoreDbContext db) BuildSut()
    {
        var db = TestUtils.NewDb();
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
        var cancelSale = new CancelSaleHandler(db, Substitute.For<ILogger<CancelSaleHandler>>(), rm);
        var cancelItem = new CancelSaleItemHandler(db, Substitute.For<ILogger<CancelSaleItemHandler>>(), rm);
        return (cancelSale, cancelItem, db);
    }

    [Fact]
    public async Task Cancel_Sale_Sets_Cancelled_True()
    {
        var (cancelSale, _, db) = BuildSut();
        var sale = new Sale { Number = "S-4000", Date = DateOnly.FromDateTime(DateTime.Today), CustomerId = 1, CustomerName = "A", BranchId = 1, BranchName = "B" };
        db.Sales.Add(sale);
        await db.SaveChangesAsync();

        await cancelSale.Handle(new CancelSaleCommand(sale.Id), CancellationToken.None);

        var found = await db.Sales.FindAsync(sale.Id);
        found!.Cancelled.Should().BeTrue(); 
    }

    [Fact]
    public async Task Cancel_Item_Recalculates_Total()
    {
        var (_, cancelItem, db) = BuildSut();
        var sale = new Sale { Number = "S-4001", Date = DateOnly.FromDateTime(DateTime.Today), CustomerId = 1, CustomerName = "A", BranchId = 1, BranchName = "B" };
        sale.Items.Add(new SaleItem { ProductId = 1, ProductName = "P1", Quantity = 4, UnitPrice = 100m, DiscountPercent = 0.10m, Total = 360m });
        sale.Items.Add(new SaleItem { ProductId = 2, ProductName = "P2", Quantity = 10, UnitPrice = 50m, DiscountPercent = 0.20m, Total = 400m });
        sale.Total = 760m;
        db.Sales.Add(sale);
        await db.SaveChangesAsync();

        await cancelItem.Handle(new CancelSaleItemCommand(sale.Id, sale.Items[1].Id), CancellationToken.None);

        var found = await db.Sales.FindAsync(sale.Id);
        found!.Total.Should().Be(360m); 
    }
}
