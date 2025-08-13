using AutoMapper;
using DeveloperStore.Application.Common;
using DeveloperStore.Application.DTOs;
using DeveloperStore.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Api.Controllers;

[ApiController]
[Route("products")]
public class ProductsController : ControllerBase
{
    private readonly DeveloperStoreDbContext _db;
    private readonly IMapper _mapper;

    public ProductsController(DeveloperStoreDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> Get(
        [FromQuery(Name = "_page")] int page = 1, 
        [FromQuery(Name = "_size")] int size = 10, 
        [FromQuery(Name = "_order")] string? order = null,
        [FromQuery] string? title = null,
        [FromQuery] string? category = null,
        [FromQuery(Name = "_minPrice")] decimal? minPrice = null,
        [FromQuery(Name = "_maxPrice")] decimal? maxPrice = null,
        [FromQuery(Name = "_minRate")] decimal? minRate = null,
        [FromQuery(Name = "_maxRate")] decimal? maxRate = null)
    {
        page = page < 1 ? 1 : page;
        size = size < 1 ? 10 : Math.Min(size, 100);

        var query = _db.Products.AsNoTracking();

        
        if (!string.IsNullOrWhiteSpace(title))
        {
            if (title.StartsWith("*") && title.EndsWith("*"))
            {
                var searchTerm = title.Trim('*');
                query = query.Where(p => p.Title.Contains(searchTerm));
            }
            else if (title.StartsWith("*"))
            {
                var searchTerm = title.TrimStart('*');
                query = query.Where(p => p.Title.EndsWith(searchTerm));
            }
            else if (title.EndsWith("*"))
            {
                var searchTerm = title.TrimEnd('*');
                query = query.Where(p => p.Title.StartsWith(searchTerm));
            }
            else
            {
                query = query.Where(p => p.Title == title);
            }
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            if (category.StartsWith("*") && category.EndsWith("*"))
            {
                var searchTerm = category.Trim('*');
                query = query.Where(p => p.Category.Contains(searchTerm));
            }
            else if (category.StartsWith("*"))
            {
                var searchTerm = category.TrimStart('*');
                query = query.Where(p => p.Category.EndsWith(searchTerm));
            }
            else if (category.EndsWith("*"))
            {
                var searchTerm = category.TrimEnd('*');
                query = query.Where(p => p.Category.StartsWith(searchTerm));
            }
            else
            {
                query = query.Where(p => p.Category == category);
            }
        }

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        if (minRate.HasValue)
            query = query.Where(p => p.Rating.Rate >= minRate.Value);

        if (maxRate.HasValue)
            query = query.Where(p => p.Rating.Rate <= maxRate.Value);

        if (!string.IsNullOrWhiteSpace(order))
        {
            query = OrderParser.ApplyOrder(query, order);
        }

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        var data = items.Select(p => _mapper.Map<ProductDto>(p));

        return Ok(new PagedResult<ProductDto>
        {
            Data = data,
            TotalItems = total,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(total / (double)size)
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p == null) return NotFound(new { type = "NotFound", error = "Product not found", detail = $"id={id}" });
        return Ok(_mapper.Map<ProductDto>(p));
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto)
    {
        var entity = _mapper.Map<DeveloperStore.Domain.Entities.Product>(dto);
        _db.Products.Add(entity);
        await _db.SaveChangesAsync();
        var result = _mapper.Map<ProductDto>(entity);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        var entity = await _db.Products.FindAsync(id);
        if (entity == null) return NotFound(new { type = "NotFound", error = "Product not found", detail = $"id={id}" });

        _mapper.Map(dto, entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Products.FindAsync(id);
        if (entity == null) return NotFound(new { type = "NotFound", error = "Product not found", detail = $"id={id}" });
        _db.Products.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<string>>> GetCategories()
    {
        var categories = await _db.Products.AsNoTracking().Select(p => p.Category).Distinct().OrderBy(x => x).ToListAsync();
        return Ok(categories);
    }

    [HttpGet("category/{category}")]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetByCategory(string category, [FromQuery(Name = "_page")] int page = 1, [FromQuery(Name = "_size")] int size = 10, [FromQuery(Name = "_order")] string? order = null)
    {
        page = page < 1 ? 1 : page;
        size = size < 1 ? 10 : Math.Min(size, 100);

        var query = _db.Products.AsNoTracking().Where(p => p.Category.ToLower() == category.ToLower());

        if (!string.IsNullOrWhiteSpace(order))
            query = OrderParser.ApplyOrder(query, order);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        var data = items.Select(p => _mapper.Map<ProductDto>(p));

        return Ok(new PagedResult<ProductDto>
        {
            Data = data,
            TotalItems = total,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(total / (double)size)
        });
    }
}
