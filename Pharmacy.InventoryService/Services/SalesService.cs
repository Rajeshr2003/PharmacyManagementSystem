using Microsoft.EntityFrameworkCore;
using Pharmacy.InventoryService.Data;
using Pharmacy.InventoryService.DTOs;
using Pharmacy.InventoryService.Entities;
using Pharmacy.InventoryService.Interfaces;

namespace Pharmacy.InventoryService.Services;

public class SalesService : ISalesService
{
    private readonly InventoryDbContext _context;

    public SalesService(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<List<Sale>> GetAllSales()
    {
        return await _context.Sales.ToListAsync();
    }

    public async Task<Sale> AddSale(SaleDto dto)
    {
        var sale = new Sale
        {
            OrderId = dto.OrderId,
            Amount = dto.Amount,
            SaleDate = DateTime.UtcNow
        };

        _context.Sales.Add(sale);

        await _context.SaveChangesAsync();

        return sale;
    }
}