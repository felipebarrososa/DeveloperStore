using DeveloperStore.Application.DTOs;
using DeveloperStore.Application.Sales;
using DeveloperStore.Infrastructure.ReadModel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperStore.Api.Controllers;

[ApiController]
[Route("sales")]
public class SalesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly SalesReadModel _rm;
    public SalesController(IMediator mediator, SalesReadModel rm)
    {
        _mediator = mediator; _rm = rm;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery(Name="_page")] int page = 1, 
        [FromQuery(Name="_size")] int size = 10,
        [FromQuery(Name="_order")] string? order = null,
        [FromQuery] DateOnly? from = null, 
        [FromQuery] DateOnly? to = null,
        [FromQuery] string? customer = null, 
        [FromQuery] string? branch = null,
        [FromQuery(Name="_minTotal")] decimal? minTotal = null,
        [FromQuery(Name="_maxTotal")] decimal? maxTotal = null,
        [FromQuery] bool? cancelled = null,
        CancellationToken ct = default)
    {
        var (data, total) = await _mediator.Send(new GetSalesQuery(page, size, order, from, to, customer, branch, minTotal, maxTotal, cancelled), ct);
        return Ok(new { data, totalItems = total, currentPage = page, totalPages = (int)Math.Ceiling(total / (double)size) });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var sale = await _mediator.Send(new GetSaleByIdQuery(id), ct);
        if (sale is null) return NotFound(new { type = "NotFound", error = "Sale not found", detail = $"id={id}" });
        return Ok(sale);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaleCreateDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateSaleCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaleUpdateDto dto, CancellationToken ct)
    {
        await _mediator.Send(new UpdateSaleCommand(id, dto), ct);
        return NoContent();
    }

    [Authorize]
    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        await _mediator.Send(new CancelSaleCommand(id), ct);
        return NoContent();
    }

    [Authorize]
    [HttpPost("{id:int}/items/{itemId:int}/cancel")]
    public async Task<IActionResult> CancelItem(int id, int itemId, CancellationToken ct)
    {
        await _mediator.Send(new CancelSaleItemCommand(id, itemId), ct);
        return NoContent();
    }

    
    [HttpGet("summary")]
    public async Task<IActionResult> Summary([FromQuery] DateOnly? from = null, [FromQuery] DateOnly? to = null, CancellationToken ct = default)
    {
        var list = await _rm.GetDailySummaryAsync(from, to, ct);
        return Ok(list);
    }
}
