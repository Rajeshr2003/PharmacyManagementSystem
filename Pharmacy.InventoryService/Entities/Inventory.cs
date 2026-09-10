namespace Pharmacy.InventoryService.Entities;

public class Inventory
{
    public int Id { get; set; }

    public string MedicineName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }
}




// namespace Pharmacy.InventoryService.Entities;

// public class Inventory
// {
//     public int Id { get; set; }

//     public int ProductId { get; set; }

//     public string ProductName { get; set; } = string.Empty;

//     public int StockQuantity { get; set; }

//     public DateTime LastUpdated { get; set; }
// }