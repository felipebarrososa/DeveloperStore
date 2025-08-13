using DeveloperStore.Application.Sales;
using DeveloperStore.Domain.Entities;
using DeveloperStore.Infrastructure.Data;
using DeveloperStore.Infrastructure.ReadModel;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DeveloperStore.Api.Sales;

public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand>
{
    private readonly DeveloperStoreDbContext _db;
    private readonly ILogger<UpdateSaleHandler> _logger;
    private readonly SalesReadModel _rm;

    public UpdateSaleHandler(DeveloperStoreDbContext db, ILogger<UpdateSaleHandler> logger, SalesReadModel rm)
    {
        _db = db; _logger = logger; _rm = rm;
    }

    
    public async Task Handle(UpdateSaleCommand request, CancellationToken ct)
    {
        var entity = await _db.Sales.Include(s => s.Items).FirstOrDefaultAsync(s => s.Id == request.Id, ct);
        if (entity is null) throw new KeyNotFoundException("Sale not found");

        var dto = request.Dto;
        entity.Number = dto.Number;
        entity.Date = dto.Date;
        entity.CustomerId = dto.CustomerId;
        entity.CustomerName = dto.CustomerName;
        entity.BranchId = dto.BranchId;
        entity.BranchName = dto.BranchName;
        entity.Cancelled = dto.Cancelled;

        _db.SaleItems.RemoveRange(entity.Items);
        entity.Items.Clear();

        foreach (var it in dto.Items)
        {
            var (discount, error) = DeveloperStore.Application.Sales.DiscountCalculator.FromQuantity(it.Quantity);
            if (error is not null) throw new InvalidOperationException(error);
            var lineTotal = it.Quantity * it.UnitPrice * (1 - discount);
            entity.Items.Add(new SaleItem
            {
                ProductId = it.ProductId,
                ProductName = it.ProductName,
                Quantity = it.Quantity,
                UnitPrice = it.UnitPrice,
                DiscountPercent = discount,
                Total = decimal.Round(lineTotal, 2),
                Cancelled = false
            });
        }

        entity.Total = entity.Items.Where(i => !i.Cancelled).Sum(i => i.Total);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("[SaleModified] {Number} total={Total}", entity.Number, entity.Total);

        await _rm.UpsertSaleAsync(new SaleDoc
        {
            Id = entity.Id,
            Number = entity.Number,
            Date = entity.Date.ToDateTime(TimeOnly.MinValue),
            CustomerId = entity.CustomerId,
            CustomerName = entity.CustomerName,
            BranchId = entity.BranchId,
            BranchName = entity.BranchName,
            Total = entity.Total,
            Cancelled = entity.Cancelled
        }, ct);
    }
}
