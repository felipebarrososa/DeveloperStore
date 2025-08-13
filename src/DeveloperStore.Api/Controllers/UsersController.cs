using AutoMapper;
using DeveloperStore.Application.Common;
using DeveloperStore.Application.DTOs;
using DeveloperStore.Domain.Enums;
using DeveloperStore.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCryptNet = BCrypt.Net.BCrypt;

namespace DeveloperStore.Api.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly DeveloperStoreDbContext _db;
    private readonly IMapper _mapper;

    public UsersController(DeveloperStoreDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<UserDto>>> Get(
        [FromQuery(Name = "_page")] int page = 1, 
        [FromQuery(Name = "_size")] int size = 10, 
        [FromQuery(Name = "_order")] string? order = null,
        [FromQuery] string? username = null,
        [FromQuery] string? email = null,
        [FromQuery] string? firstname = null,
        [FromQuery] string? lastname = null,
        [FromQuery] string? city = null,
        [FromQuery] UserStatus? status = null,
        [FromQuery] UserRole? role = null)
    {
        page = page < 1 ? 1 : page;
        size = size < 1 ? 10 : Math.Min(size, 100);
        var query = _db.Users.AsNoTracking();

       
        if (!string.IsNullOrWhiteSpace(username))
        {
            if (username.StartsWith("*") && username.EndsWith("*"))
            {
                var searchTerm = username.Trim('*');
                query = query.Where(u => u.Username.Contains(searchTerm));
            }
            else if (username.StartsWith("*"))
            {
                var searchTerm = username.TrimStart('*');
                query = query.Where(u => u.Username.EndsWith(searchTerm));
            }
            else if (username.EndsWith("*"))
            {
                var searchTerm = username.TrimEnd('*');
                query = query.Where(u => u.Username.StartsWith(searchTerm));
            }
            else
            {
                query = query.Where(u => u.Username == username);
            }
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            if (email.StartsWith("*") && email.EndsWith("*"))
            {
                var searchTerm = email.Trim('*');
                query = query.Where(u => u.Email.Contains(searchTerm));
            }
            else if (email.StartsWith("*"))
            {
                var searchTerm = email.TrimStart('*');
                query = query.Where(u => u.Email.EndsWith(searchTerm));
            }
            else if (email.EndsWith("*"))
            {
                var searchTerm = email.TrimEnd('*');
                query = query.Where(u => u.Email.StartsWith(searchTerm));
            }
            else
            {
                query = query.Where(u => u.Email == email);
            }
        }

        if (!string.IsNullOrWhiteSpace(firstname))
        {
            if (firstname.StartsWith("*") && firstname.EndsWith("*"))
            {
                var searchTerm = firstname.Trim('*');
                query = query.Where(u => u.Name.Firstname.Contains(searchTerm));
            }
            else if (firstname.StartsWith("*"))
            {
                var searchTerm = firstname.TrimStart('*');
                query = query.Where(u => u.Name.Firstname.EndsWith(searchTerm));
            }
            else if (firstname.EndsWith("*"))
            {
                var searchTerm = firstname.TrimEnd('*');
                query = query.Where(u => u.Name.Firstname.StartsWith(searchTerm));
            }
            else
            {
                query = query.Where(u => u.Name.Firstname == firstname);
            }
        }

        if (!string.IsNullOrWhiteSpace(lastname))
        {
            if (lastname.StartsWith("*") && lastname.EndsWith("*"))
            {
                var searchTerm = lastname.Trim('*');
                query = query.Where(u => u.Name.Lastname.Contains(searchTerm));
            }
            else if (lastname.StartsWith("*"))
            {
                var searchTerm = lastname.TrimStart('*');
                query = query.Where(u => u.Name.Lastname.EndsWith(searchTerm));
            }
            else if (lastname.EndsWith("*"))
            {
                var searchTerm = lastname.TrimEnd('*');
                query = query.Where(u => u.Name.Lastname.StartsWith(searchTerm));
            }
            else
            {
                query = query.Where(u => u.Name.Lastname == lastname);
            }
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            if (city.StartsWith("*") && city.EndsWith("*"))
            {
                var searchTerm = city.Trim('*');
                query = query.Where(u => u.Address.City.Contains(searchTerm));
            }
            else if (city.StartsWith("*"))
            {
                var searchTerm = city.TrimStart('*');
                query = query.Where(u => u.Address.City.EndsWith(searchTerm));
            }
            else if (city.EndsWith("*"))
            {
                var searchTerm = city.TrimEnd('*');
                query = query.Where(u => u.Address.City.StartsWith(searchTerm));
            }
            else
            {
                query = query.Where(u => u.Address.City == city);
            }
        }

        if (status.HasValue)
            query = query.Where(u => u.Status == status.Value);

        if (role.HasValue)
            query = query.Where(u => u.Role == role.Value);

        if (!string.IsNullOrWhiteSpace(order))
            query = Application.Common.OrderParser.ApplyOrder(query, order);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        var data = items.Select(u => _mapper.Map<UserDto>(u));

        return Ok(new PagedResult<UserDto>
        {
            Data = data,
            TotalItems = total,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(total / (double)size)
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var entity = await _db.Users.FindAsync(id);
        if (entity == null) return NotFound(new { type = "NotFound", error = "User not found", detail = $"id={id}" });
        return Ok(_mapper.Map<UserDto>(entity));
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserDto dto)
    {
        var exists = await _db.Users.AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email);
        if (exists) return Conflict(new { type = "Conflict", error = "User exists", detail = "Email or username already used" });

        var user = new DeveloperStore.Domain.Entities.User
        {
            Email = dto.Email,
            Username = dto.Username,
            PasswordHash = BCryptNet.HashPassword(dto.Password),
            Name = new DeveloperStore.Domain.ValueObjects.Name { Firstname = dto.Firstname, Lastname = dto.Lastname },
            Address = new DeveloperStore.Domain.ValueObjects.Address
            {
                City = dto.City, Street = dto.Street, Number = dto.Number, Zipcode = dto.Zipcode,
                Geo = new DeveloperStore.Domain.ValueObjects.Geo { Lat = dto.Lat, Long = dto.Long }
            },
            Phone = dto.Phone,
            Status = dto.Status,
            Role = dto.Role
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, _mapper.Map<UserDto>(user));
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateUserDto dto)
    {
        var entity = await _db.Users.FindAsync(id);
        if (entity == null) return NotFound(new { type = "NotFound", error = "User not found", detail = $"id={id}" });

        
        var emailTaken = await _db.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id);
        var usernameTaken = await _db.Users.AnyAsync(u => u.Username == dto.Username && u.Id != id);
        if (emailTaken || usernameTaken) return Conflict(new { type = "Conflict", error = "User exists", detail = "Email or username already used" });

        entity.Email = dto.Email;
        entity.Username = dto.Username;
        if (!string.IsNullOrWhiteSpace(dto.Password))
            entity.PasswordHash = BCryptNet.HashPassword(dto.Password);
        entity.Name.Firstname = dto.Firstname;
        entity.Name.Lastname = dto.Lastname;
        entity.Address.City = dto.City;
        entity.Address.Street = dto.Street;
        entity.Address.Number = dto.Number;
        entity.Address.Zipcode = dto.Zipcode;
        entity.Address.Geo.Lat = dto.Lat;
        entity.Address.Geo.Long = dto.Long;
        entity.Phone = dto.Phone;
        entity.Status = dto.Status;
        entity.Role = dto.Role;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Users.FindAsync(id);
        if (entity == null) return NotFound(new { type = "NotFound", error = "User not found", detail = $"id={id}" });
        _db.Users.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
