using Pharmacy.InventoryService.DTOs;
using Pharmacy.InventoryService.Entities;

namespace Pharmacy.InventoryService.Interfaces;

public interface IInventoryService
{
    Task<Inventory> AddMedicine(AddMedicineDto dto);

    Task<List<Inventory>> GetAllMedicines();

    Task<Inventory?> GetMedicineById(int id);

    Task<Inventory?> UpdateMedicine(
        int id,
        UpdateMedicineDto dto);

    Task<bool> DeleteMedicine(int id);
    Task<bool> HasStock(int medicineId, int quantity);
    Task<bool> ReduceStock(int medicineId, int quantity);
}