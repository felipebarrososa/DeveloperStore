using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using DeveloperStore.Api.Sales;
using DeveloperStore.Application.Sales;
using DeveloperStore.Domain.Entities;
using DeveloperStore.UnitTests.Helpers;
using FluentAssertions;
using Xunit;

namespace DeveloperStore.UnitTests.Sales;

public class GetSalesHandlerTests
{
    [Fact]
    public async Task Paginates_And_Filters_By_Customer_And_Branch()
    {
        var db = TestUtils.NewDb();
        var mapper = TestUtils.NewMapper();
        var h = new GetSalesHandler(db, mapper);

        // seed: Ana+Centro aparecem nos i: 0, 6, 12 (3 itens)
        for (int i = 0; i < 15; i++)
        {
            db.Sales.Add(new Sale
            {
                Number = $"S-{i + 1:000}",
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-i)),
                CustomerId = 1,
                CustomerName = i % 2 == 0 ? "Ana" : "Bruno",
                BranchId = 1,
                BranchName = i % 3 == 0 ? "Centro" : "Zona Sul",
                Total = 100m * (i + 1)
            });
        }
        await db.SaveChangesAsync();

        // Page=1 (senão a coleção filtrada fica vazia)
        var (data, total) = await h.Handle(
            new GetSalesQuery(Page: 1, Size: 5, Order: null, From: null, To: null, Customer: "ana", Branch: "centro"),
            CancellationToken.None);

        total.Should().BeGreaterThan(0);
        data.Should().OnlyContain(s => s.CustomerName.ToLower().Contains("ana") && s.BranchName.ToLower().Contains("centro"));
        data.Count().Should().BeLessOrEqualTo(5);
    }

    [Fact]
    public async Task Get_By_Id_Returns_Null_When_Not_Found()
    {
        var db = TestUtils.NewDb();
        var mapper = TestUtils.NewMapper();
        var h = new GetSaleByIdHandler(db, mapper);

        var res = await h.Handle(new GetSaleByIdQuery(999), CancellationToken.None);
        res.Should().BeNull();
    }
}
