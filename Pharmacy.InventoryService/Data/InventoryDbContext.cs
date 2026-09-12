using Microsoft.EntityFrameworkCore;
using Pharmacy.InventoryService.Entities;

namespace Pharmacy.InventoryService.Data;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(
        DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Supplier> Suppliers { get; set; } = null!;

    public DbSet<Sale> Sales { get; set; } = null!;

    public DbSet<Inventory> Inventories { get; set; } = null!;

    // protected override void OnModelCreating(ModelBuilder modelBuilder)
    // {
    //     base.OnModelCreating(modelBuilder);

    //     modelBuilder.Entity<Sale>()
    //         .HasIndex(sale => sale.OrderId)
    //         .IsUnique();
    // }
}