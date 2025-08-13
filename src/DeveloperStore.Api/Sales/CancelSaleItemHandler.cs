using DeveloperStore.Application.Sales;
using DeveloperStore.Infrastructure.Data;
using DeveloperStore.Infrastructure.ReadModel;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DeveloperStore.Api.Sales;

public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand>
{
    private readonly DeveloperStoreDbContext _db;
    private readonly ILogger<CancelSaleItemHandler> _logger;
    private readonly SalesReadModel _rm;

    public CancelSaleItemHandler(DeveloperStoreDbContext db, ILogger<CancelSaleItemHandler> logger, SalesReadModel rm)
    {
        _db = db; _logger = logger; _rm = rm;
    }

    public async Task Handle(CancelSaleItemCommand request, CancellationToken ct)
    {
        var s = await _db.Sales.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (s is null) throw new KeyNotFoundException("Sale not found");

        var item = s.Items.FirstOrDefault(i => i.Id == request.ItemId);
        if (item is null) throw new KeyNotFoundException("Sale item not found");

        item.Cancelled = true;
        s.Total = s.Items.Where(i => !i.Cancelled).Sum(i => i.Total);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("[ItemCancelled] sale={Number} item={ItemId}", s.Number, item.Id);

        await _rm.UpsertSaleAsync(new SaleDoc
        {
            Id = s.Id,
            Number = s.Number,
            Date = s.Date.ToDateTime(TimeOnly.MinValue),
            CustomerId = s.CustomerId,
            CustomerName = s.CustomerName,
            BranchId = s.BranchId,
            BranchName = s.BranchName,
            Total = s.Total,
            Cancelled = s.Cancelled
        }, ct);
    }
}
