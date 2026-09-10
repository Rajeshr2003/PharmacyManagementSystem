using Pharmacy.InventoryService.DTOs;
using Pharmacy.InventoryService.Entities;

namespace Pharmacy.InventoryService.Interfaces;

public interface ISalesService
{
    Task<List<Sale>> GetAllSales();

    Task<Sale> AddSale(SaleDto dto);
}