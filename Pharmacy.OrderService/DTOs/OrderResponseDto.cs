namespace Pharmacy.OrderService.DTOs;

public class OrderResponseDto
{
    public int OrderId { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = string.Empty;
}