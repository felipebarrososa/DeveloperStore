using DeveloperStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Infrastructure.Data;

public class DeveloperStoreDbContext : DbContext
{
    public DeveloperStoreDbContext(DbContextOptions<DeveloperStoreDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        
        modelBuilder.Entity<Product>().OwnsOne(p => p.Rating);

        modelBuilder.Entity<User>().OwnsOne(u => u.Name);
        modelBuilder.Entity<User>().OwnsOne(u => u.Address, a =>
        {
            a.OwnsOne(x => x.Geo);
        });

        modelBuilder.Entity<CartItem>()
            .HasOne<Cart>()
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Sale>().HasIndex(s => s.Number).IsUnique();
        modelBuilder.Entity<SaleItem>()
            .HasOne<Sale>()
            .WithMany(s => s.Items)
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
