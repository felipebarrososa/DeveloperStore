using AutoMapper;
using DeveloperStore.Application.DTOs;
using DeveloperStore.Application.Sales;
using DeveloperStore.Domain.Entities;
using DeveloperStore.Infrastructure.Data;
using DeveloperStore.Infrastructure.ReadModel;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DeveloperStore.Api.Sales;

public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, SaleDto>
{
    private readonly DeveloperStoreDbContext _db;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateSaleHandler> _logger;
    private readonly SalesReadModel _rm;

    public CreateSaleHandler(DeveloperStoreDbContext db, IMapper mapper, ILogger<CreateSaleHandler> logger, SalesReadModel rm)
    {
        _db = db; _mapper = mapper; _logger = logger; _rm = rm;
    }

    public async Task<SaleDto> Handle(CreateSaleCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        if (await _db.Sales.AnyAsync(s => s.Number == dto.Number, ct))
            throw new InvalidOperationException($"Sale number '{dto.Number}' already exists.");

        var sale = new Sale
        {
            Number = dto.Number,
            Date = dto.Date,
            CustomerId = dto.CustomerId,
            CustomerName = dto.CustomerName,
            BranchId = dto.BranchId,
            BranchName = dto.BranchName,
            Cancelled = false,
            Items = new List<SaleItem>()
        };

        foreach (var it in dto.Items)
        {
            var (discount, error) = DiscountCalculator.FromQuantity(it.Quantity);
            if (error is not null) throw new InvalidOperationException(error);
            var lineTotal = it.Quantity * it.UnitPrice * (1 - discount);
            sale.Items.Add(new SaleItem
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

        sale.Total = sale.Items.Where(i => !i.Cancelled).Sum(i => i.Total);
        _db.Sales.Add(sale);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("[SaleCreated] {Number} total={Total}", sale.Number, sale.Total);

        await _rm.UpsertSaleAsync(new SaleDoc
        {
            Id = sale.Id,
            Number = sale.Number,
            Date = sale.Date.ToDateTime(TimeOnly.MinValue),
            CustomerId = sale.CustomerId,
            CustomerName = sale.CustomerName,
            BranchId = sale.BranchId,
            BranchName = sale.BranchName,
            Total = sale.Total,
            Cancelled = sale.Cancelled
        }, ct);

        return _mapper.Map<SaleDto>(sale);
    }
}
