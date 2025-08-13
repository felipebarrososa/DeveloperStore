using Bogus;
using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using BCryptNet = BCrypt.Net.BCrypt;

namespace DeveloperStore.Infrastructure.Seed;

public static class SeedData
{
    public static async Task EnsureSeedAsync(DbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        var ctx = (DeveloperStore.Infrastructure.Data.DeveloperStoreDbContext)db;

        if (!ctx.Products.Any())
        {
            var categories = new[] { "electronics", "jewelery", "men's clothing", "women's clothing" };
            var productFaker = new Faker<Product>("en")
                .RuleFor(p => p.Title, f => f.Commerce.ProductName())
                .RuleFor(p => p.Price, f => decimal.Parse(f.Commerce.Price(10, 500)))
                .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
                .RuleFor(p => p.Category, f => f.PickRandom(categories))
                .RuleFor(p => p.Image, f => f.Image.PicsumUrl())
                .RuleFor(p => p.Rating, f => new ProductRating { Rate = Math.Round((decimal)f.Random.Double(1, 5), 1), Count = f.Random.Int(0, 1000) });

            var products = productFaker.Generate(25);
            ctx.Products.AddRange(products);
        }

        if (!ctx.Users.Any())
        {
            var userFaker = new Faker<User>("en")
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.Username, f => f.Internet.UserName())
                .RuleFor(u => u.PasswordHash, f => BCryptNet.HashPassword("Pass@123"))
                .RuleFor(u => u.Name, f => new Domain.ValueObjects.Name { Firstname = f.Name.FirstName(), Lastname = f.Name.LastName() })
                .RuleFor(u => u.Address, f => new Domain.ValueObjects.Address
                {
                    City = f.Address.City(),
                    Street = f.Address.StreetName(),
                    Number = f.Address.BuildingNumber(),
                    Zipcode = f.Address.ZipCode(),
                    Geo = new Domain.ValueObjects.Geo { Lat = f.Address.Latitude().ToString("0.0000"), Long = f.Address.Longitude().ToString("0.0000") }
                })
                .RuleFor(u => u.Phone, f => f.Phone.PhoneNumber())
                .RuleFor(u => u.Status, f => f.PickRandom<UserStatus>())
                .RuleFor(u => u.Role, f => UserRole.Customer);

            var users = userFaker.Generate(5);

            
            users.Add(new User
            {
                Email = "admin@devstore.com",
                Username = "admin",
                PasswordHash = BCryptNet.HashPassword("Pass@123"),
                Name = new Domain.ValueObjects.Name { Firstname = "System", Lastname = "Admin" },
                Address = new Domain.ValueObjects.Address { City = "SP", Street = "Central", Number = "100", Zipcode = "00000-000", Geo = new Domain.ValueObjects.Geo { Lat = "0", Long = "0" } },
                Phone = "0000-0000",
                Status = UserStatus.Active,
                Role = UserRole.Admin
            });
            users.Add(new User
            {
                Email = "manager@devstore.com",
                Username = "manager",
                PasswordHash = BCryptNet.HashPassword("Pass@123"),
                Name = new Domain.ValueObjects.Name { Firstname = "Store", Lastname = "Manager" },
                Address = new Domain.ValueObjects.Address { City = "SP", Street = "Central", Number = "101", Zipcode = "00000-001", Geo = new Domain.ValueObjects.Geo { Lat = "0", Long = "0" } },
                Phone = "0000-0001",
                Status = UserStatus.Active,
                Role = UserRole.Manager
            });
            users.Add(new User
            {
                Email = "user@devstore.com",
                Username = "user",
                PasswordHash = BCryptNet.HashPassword("Pass@123"),
                Name = new Domain.ValueObjects.Name { Firstname = "Basic", Lastname = "User" },
                Address = new Domain.ValueObjects.Address { City = "SP", Street = "Central", Number = "102", Zipcode = "00000-002", Geo = new Domain.ValueObjects.Geo { Lat = "0", Long = "0" } },
                Phone = "0000-0002",
                Status = UserStatus.Active,
                Role = UserRole.Customer
            });

            ctx.Users.AddRange(users);
        }

        if (!ctx.Carts.Any())
        {
            var users = ctx.Users.ToList();
            var products = ctx.Products.ToList();
            if (users.Any() && products.Any())
            {
                var rnd = new Random();
                for (int i = 0; i < 5; i++)
                {
                    var user = users[rnd.Next(users.Count)];
                    var cart = new Cart
                    {
                        UserId = user.Id,
                        Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-rnd.Next(1, 30)))
                    };
                    int items = rnd.Next(1, 4);
                    for (int j = 0; j < items; j++)
                    {
                        var prod = products[rnd.Next(products.Count)];
                        cart.Items.Add(new CartItem
                        {
                            ProductId = prod.Id,
                            Quantity = rnd.Next(1, 5)
                        });
                    }
                    ctx.Carts.Add(cart);
                }
            }
        }

        await ctx.SaveChangesAsync();
    }
}
