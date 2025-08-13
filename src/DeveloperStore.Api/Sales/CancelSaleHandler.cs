using DeveloperStore.Application.Sales;
using DeveloperStore.Infrastructure.Data;
using DeveloperStore.Infrastructure.ReadModel;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DeveloperStore.Api.Sales;

public class CancelSaleHandler : IRequestHandler<CancelSaleCommand>
{
    private readonly DeveloperStoreDbContext _db;
    private readonly ILogger<CancelSaleHandler> _logger;
    private readonly SalesReadModel _rm;

    public CancelSaleHandler(DeveloperStoreDbContext db, ILogger<CancelSaleHandler> logger, SalesReadModel rm)
    {
        _db = db; _logger = logger; _rm = rm;
    }

    public async Task Handle(CancelSaleCommand request, CancellationToken ct)
    {
        var s = await _db.Sales.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (s is null) throw new KeyNotFoundException("Sale not found");

        s.Cancelled = true;
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("[SaleCancelled] {Number}", s.Number);

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
