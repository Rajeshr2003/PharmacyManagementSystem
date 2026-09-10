using Microsoft.EntityFrameworkCore;
using Pharmacy.InventoryService.Data;
using Pharmacy.InventoryService.DTOs;
using Pharmacy.InventoryService.Entities;
using Pharmacy.InventoryService.Interfaces;

namespace Pharmacy.InventoryService.Services;

public class SupplierService : ISupplierService
{
    private readonly InventoryDbContext _context;

    public SupplierService(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<List<Supplier>> GetAll()
    {
        return await _context.Suppliers.ToListAsync();
    }

    public async Task<Supplier?> GetById(int id)
    {
        return await _context.Suppliers.FindAsync(id);
    }

    public async Task<Supplier> Add(SupplierDto dto)
    {
        var supplier = new Supplier
        {
            Name = dto.Name,
            Contact = dto.Contact,
            Email = dto.Email
        };

        _context.Suppliers.Add(supplier);

        await _context.SaveChangesAsync();

        return supplier;
    }

    public async Task<Supplier?> Update(
        int id,
        SupplierDto dto)
    {
        var supplier =
            await _context.Suppliers.FindAsync(id);

        if (supplier == null)
            return null;

        supplier.Name = dto.Name;
        supplier.Contact = dto.Contact;
        supplier.Email = dto.Email;

        await _context.SaveChangesAsync();

        return supplier;
    }

    public async Task<bool> Delete(int id)
    {
        var supplier =
            await _context.Suppliers.FindAsync(id);

        if (supplier == null)
            return false;

        _context.Suppliers.Remove(supplier);

        await _context.SaveChangesAsync();

        return true;
    }
}
