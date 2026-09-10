namespace Pharmacy.OrderService.DTOs;

public class CreateOrderDto
{
    public List<CreateOrderItemDto> Items { get; set; } = new();
}





// namespace Pharmacy.OrderService.DTOs;

// public class CreateOrderDto
// {
//     public int UserId { get; set; }

//     public List<CreateOrderItemDto> Items { get; set; }
//         = new();
// }