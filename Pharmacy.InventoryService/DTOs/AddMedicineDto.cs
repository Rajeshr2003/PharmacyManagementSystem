namespace Pharmacy.InventoryService.DTOs;

public class AddMedicineDto
{
    public string MedicineName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

}