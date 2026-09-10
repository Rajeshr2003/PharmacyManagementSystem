using Microsoft.EntityFrameworkCore;
using Pharmacy.OrderService.Entities;

namespace Pharmacy.OrderService.Data;

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(
        DbContextOptions<OrdersDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }

    public DbSet<OrderItem> OrderItems { get; set; }
}