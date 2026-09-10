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

    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
}