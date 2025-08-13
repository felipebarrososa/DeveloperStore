using AutoMapper;
using DeveloperStore.Application.DTOs;
using DeveloperStore.Application.Sales;
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
        _db = db; _mapper = mapper;
    }

    public async Task<(IEnumerable<SaleDto> data, int total)> Handle(GetSalesQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var size = request.Size < 1 ? 10 : Math.Min(request.Size, 100);

        var q = _db.Sales.Include(x => x.Items).AsNoTracking().AsQueryable();

        if (request.From.HasValue) q = q.Where(s => s.Date >= request.From.Value);
        if (request.To.HasValue) q = q.Where(s => s.Date <= request.To.Value);
        if (!string.IsNullOrWhiteSpace(request.Customer))
            q = q.Where(s => s.CustomerName.ToLower().Contains(request.Customer.ToLower()));
        if (!string.IsNullOrWhiteSpace(request.Branch))
            q = q.Where(s => s.BranchName.ToLower().Contains(request.Branch.ToLower()));

        // Ordem padrão
        q = q.OrderBy(s => s.Id);

        var total = await q.CountAsync(ct);
        var list = await q.Skip((page - 1) * size).Take(size).ToListAsync(ct);

        return (list.Select(s => _mapper.Map<SaleDto>(s)), total);
    }
}
