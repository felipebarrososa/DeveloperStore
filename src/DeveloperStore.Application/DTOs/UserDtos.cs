using DeveloperStore.Domain.Enums;

namespace DeveloperStore.Application.DTOs;

// ===== Saída =====
public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Firstname { get; set; } = default!;
    public string Lastname { get; set; } = default!;
    public string City { get; set; } = default!;
    public string Street { get; set; } = default!;
    public string Number { get; set; } = default!;
    public string Zipcode { get; set; } = default!;
    public string Lat { get; set; } = default!;
    public string Long { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public UserStatus Status { get; set; }
    public UserRole Role { get; set; }

    public UserDto() { }
}

// ===== Entrada =====
public record CreateUserDto(
    string Email, string Username, string Password,
    string Firstname, string Lastname,
    string City, string Street, string Number, string Zipcode,
    string Lat, string Long, string Phone, UserStatus Status, UserRole Role);

public record UpdateUserDto(
    string Email, string Username, string? Password,
    string Firstname, string Lastname,
    string City, string Street, string Number, string Zipcode,
    string Lat, string Long, string Phone, UserStatus Status, UserRole Role);
