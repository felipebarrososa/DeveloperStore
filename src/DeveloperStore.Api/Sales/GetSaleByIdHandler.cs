using AutoMapper;
using DeveloperStore.Application.DTOs;
using DeveloperStore.Application.Sales;
using DeveloperStore.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Api.Sales;

public class GetSaleByIdHandler : IRequestHandler<GetSaleByIdQuery, SaleDto?>
{
    private readonly DeveloperStoreDbContext _db;
    private readonly IMapper _mapper;

    public GetSaleByIdHandler(DeveloperStoreDbContext db, IMapper mapper)
    {
        _db = db; _mapper = mapper;
    }

    public async Task<SaleDto?> Handle(GetSaleByIdQuery request, CancellationToken ct)
    {
        var s = await _db.Sales.Include(x => x.Items)
                               .AsNoTracking()
                               .FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        return s is null ? null : _mapper.Map<SaleDto>(s);
    }
}
