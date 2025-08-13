using AutoMapper;
using DeveloperStore.Application.DTOs;
using DeveloperStore.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Api.Controllers;

[ApiController]
[Route("carts")]
public class CartsController : ControllerBase
{
    private readonly DeveloperStoreDbContext _db;
    private readonly IMapper _mapper;

    public CartsController(DeveloperStoreDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CartDto>>> Get([FromQuery(Name = "_page")] int page = 1, [FromQuery(Name = "_size")] int size = 10)
    {
        page = page < 1 ? 1 : page;
        size = size < 1 ? 10 : Math.Min(size, 100);

        var query = _db.Carts.Include(c => c.Items).AsNoTracking();
        var total = await query.CountAsync();
        var carts = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        var data = carts.Select(c => _mapper.Map<CartDto>(c));
        return Ok(new { data, totalItems = total, currentPage = page, totalPages = (int)Math.Ceiling(total / (double)size) });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CartDto>> GetById(int id)
    {
        var entity = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id);
        if (entity == null) return NotFound(new { type = "NotFound", error = "Cart not found", detail = $"id={id}" });
        return Ok(_mapper.Map<CartDto>(entity));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<CartDto>> Create(CreateCartDto dto)
    {
        // Basic validation: user exists, products exist
        var userExists = await _db.Users.AnyAsync(u => u.Id == dto.UserId);
        if (!userExists) return UnprocessableEntity(new { type = "ValidationError", error = "Invalid userId", detail = $"userId={dto.UserId}" });

        var productIds = dto.Products.Select(p => p.ProductId).Distinct().ToList();
        var found = await _db.Products.Where(p => productIds.Contains(p.Id)).Select(p => p.Id).ToListAsync();
        var missing = productIds.Except(found).ToList();
        if (missing.Any()) return UnprocessableEntity(new { type = "ValidationError", error = "Invalid productId", detail = $"missing={string.Join(',', missing)}" });

        var cart = new DeveloperStore.Domain.Entities.Cart
        {
            UserId = dto.UserId,
            Date = dto.Date,
            Items = dto.Products.Select(p => new DeveloperStore.Domain.Entities.CartItem { ProductId = p.ProductId, Quantity = p.Quantity }).ToList()
        };
        _db.Carts.Add(cart);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = cart.Id }, _mapper.Map<CartDto>(cart));
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCartDto dto)
    {
        var entity = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id);
        if (entity == null) return NotFound(new { type = "NotFound", error = "Cart not found", detail = $"id={id}" });

        entity.UserId = dto.UserId;
        entity.Date = dto.Date;
        // replace items
        _db.CartItems.RemoveRange(entity.Items);
        entity.Items = dto.Products.Select(p => new DeveloperStore.Domain.Entities.CartItem { ProductId = p.ProductId, Quantity = p.Quantity }).ToList();
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Carts.FindAsync(id);
        if (entity == null) return NotFound(new { type = "NotFound", error = "Cart not found", detail = $"id={id}" });
        _db.Carts.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
