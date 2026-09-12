using System.ComponentModel.DataAnnotations;

namespace Pharmacy.InventoryService.DTOs;

public class UpdateMedicineDto
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string MedicineName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 100000)]
    public decimal Price { get; set; }

    [Range(0, 1000000)]
    public int StockQuantity { get; set; }
}