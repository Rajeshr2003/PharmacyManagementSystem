using Pharmacy.InventoryService.DTOs;
using Pharmacy.InventoryService.Entities;

namespace Pharmacy.InventoryService.Interfaces;

public interface ISupplierService
{
    Task<List<Supplier>> GetAll();

    Task<Supplier?> GetById(int id);

    Task<Supplier> Add(SupplierDto dto);

    Task<Supplier?> Update(int id, SupplierDto dto);

    Task<bool> Delete(int id);
}
