using Microsoft.EntityFrameworkCore;
using Pharmacy.InventoryService.Data;
using Pharmacy.InventoryService.DTOs;
using Pharmacy.InventoryService.Entities;
using Pharmacy.InventoryService.Interfaces;

namespace Pharmacy.InventoryService.Services;

public class InventoryService : IInventoryService
{
    private readonly InventoryDbContext _context;

    public InventoryService(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<Inventory> AddMedicine(AddMedicineDto dto)
    {
        var medicine = new Inventory
        {
            MedicineName = dto.MedicineName,
            Description = dto.Description,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity
        };

        _context.Inventories.Add(medicine);
        await _context.SaveChangesAsync();

        return medicine;
    }

    public async Task<List<Inventory>> GetAllMedicines()
    {
        return await _context.Inventories.ToListAsync();
    }

    public async Task<Inventory?> GetMedicineById(int id)
    {
        return await _context.Inventories
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Inventory?> UpdateMedicine(
        int id,
        UpdateMedicineDto dto)
    {
        var medicine = await _context.Inventories.FindAsync(id);

        if (medicine == null)
            return null;

        medicine.MedicineName = dto.MedicineName;
        medicine.Description = dto.Description;
        medicine.Price = dto.Price;
        medicine.StockQuantity = dto.StockQuantity;

        await _context.SaveChangesAsync();

        return medicine;
    }

    public async Task<bool> DeleteMedicine(int id)
    {
        var medicine = await _context.Inventories.FindAsync(id);

        if (medicine == null)
            return false;

        _context.Inventories.Remove(medicine);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> HasStock(
    int medicineId,
    int quantity)
    { 
        var medicine = await _context.Inventories
            .FindAsync(medicineId);

        if (medicine == null)
            return false;

        return medicine.StockQuantity >= quantity;
    }

    public async Task<bool> ReduceStock(
    int medicineId,
    int quantity)
{
    var medicine = await _context.Inventories
        .FindAsync(medicineId);

    if (medicine == null)
        return false;

    if (medicine.StockQuantity < quantity)
        return false;

    medicine.StockQuantity -= quantity;

    await _context.SaveChangesAsync();

    return true;
}


}