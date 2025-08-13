using AutoMapper;
using DeveloperStore.Application.Sales;
using DeveloperStore.Application.DTOs;
using DeveloperStore.Domain.Entities;
using DeveloperStore.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Api.Sales;

public class GetSalesHandler : IRequestHandler<GetSalesQuery, (IEnumerable<SaleDto> data, int total)>
{
    private readonly DeveloperStoreDbContext _db;
    private readonly IMapper _mapper;

    public GetSalesHandler(DeveloperStoreDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<(IEnumerable<SaleDto> data, int total)> Handle(GetSalesQuery request, CancellationToken ct)
    {
        var query = _db.Sales.Include(s => s.Items).AsNoTracking();

        if (request.From.HasValue)
            query = query.Where(s => s.Date >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(s => s.Date <= request.To.Value);

        if (!string.IsNullOrWhiteSpace(request.Customer))
        {
            if (request.Customer.StartsWith("*") && request.Customer.EndsWith("*"))
            {
                var searchTerm = request.Customer.Trim('*');
                query = query.Where(s => s.CustomerName.Contains(searchTerm));
            }
            else if (request.Customer.StartsWith("*"))
            {
                var searchTerm = request.Customer.TrimStart('*');
                query = query.Where(s => s.CustomerName.EndsWith(searchTerm));
            }
            else if (request.Customer.EndsWith("*"))
            {
                var searchTerm = request.Customer.TrimEnd('*');
                query = query.Where(s => s.CustomerName.StartsWith(searchTerm));
            }
            else
            {
                query = query.Where(s => s.CustomerName == request.Customer);
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Branch))
        {
            if (request.Branch.StartsWith("*") && request.Branch.EndsWith("*"))
            {
                var searchTerm = request.Branch.Trim('*');
                query = query.Where(s => s.BranchName.Contains(searchTerm));
            }
            else if (request.Branch.StartsWith("*"))
            {
                var searchTerm = request.Branch.TrimStart('*');
                query = query.Where(s => s.BranchName.EndsWith(searchTerm));
            }
            else if (request.Branch.EndsWith("*"))
            {
                var searchTerm = request.Branch.TrimEnd('*');
                query = query.Where(s => s.BranchName.StartsWith(searchTerm));
            }
            else
            {
                query = query.Where(s => s.BranchName == request.Branch);
            }
        }

        if (request.MinTotal.HasValue)
            query = query.Where(s => s.Total >= request.MinTotal.Value);

        if (request.MaxTotal.HasValue)
            query = query.Where(s => s.Total <= request.MaxTotal.Value);

        if (request.Cancelled.HasValue)
            query = query.Where(s => s.Cancelled == request.Cancelled.Value);

        if (!string.IsNullOrWhiteSpace(request.Order))
        {
            query = DeveloperStore.Application.Common.OrderParser.ApplyOrder(query, request.Order);
        }

        var total = await query.CountAsync(ct);
        var items = await query.Skip((request.Page - 1) * request.Size).Take(request.Size).ToListAsync(ct);
        var data = items.Select(s => _mapper.Map<SaleDto>(s));

        return (data, total);
    }
}
