using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pharmacy.OrderService.Data;
using Pharmacy.OrderService.DTOs;
using Pharmacy.OrderService.Entities;
using System.Net.Http.Json;

namespace Pharmacy.OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly OrdersDbContext _context;
    private readonly HttpClient _httpClient;
    public OrderController(
    OrdersDbContext context,
    IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient();
    }

    [Authorize(Roles = "Doctor")]
    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
    {
        var order = new Order
        {
            UserId = dto.UserId,
            OrderDate = DateTime.UtcNow,
            Status = "Pending",
            TotalAmount = dto.Items.Sum(x => x.Quantity * x.UnitPrice)
        };

        foreach (var item in dto.Items)
        {
            order.OrderItems.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            });
        }

    foreach (var item in dto.Items)
    {
        var response = await _httpClient.GetAsync(
        $"https://localhost:7001/api/Inventory/check/{item.ProductId}/{item.Quantity}");

        var available =
        await response.Content.ReadFromJsonAsync<bool>();

    if (!available)
    {
        return BadRequest(
            $"Insufficient stock for Product {item.ProductId}");
    }
    }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return Ok(new OrderResponseDto
        {
            OrderId = order.Id,
            TotalAmount = order.TotalAmount,
            Status = order.Status
        });
    }
    [Authorize(Roles = "Admin,Doctor")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        return Ok(order);
    }
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .ToListAsync();

    return Ok(orders);
    }
//     [Authorize]
//     [HttpPut("{id}/status")]
// public async Task<IActionResult> UpdateStatus(int id, string status)
// {
//     var order = await _context.Orders.FindAsync(id);

//     if (order == null)
//         return NotFound();

//     order.Status = status;

//     await _context.SaveChangesAsync();

//     return Ok(order);
// }
[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteOrder(int id)
{
    var order = await _context.Orders.FindAsync(id);

    if (order == null)
        return NotFound();

    _context.Orders.Remove(order);

    await _context.SaveChangesAsync();

    return NoContent();
}

[Authorize(Roles = "Admin")]
[HttpPut("{id}/verify")]
public async Task<IActionResult> VerifyOrder(int id)
{
    var order = await _context.Orders.FindAsync(id);

    if (order == null)
        return NotFound();

    if (order.Status != "Pending")
        return BadRequest("Only Pending orders can be verified");

    order.Status = "Verified";

    await _context.SaveChangesAsync();

    return Ok(new
    {
        order.Id,
        order.Status,
        Message = "Order Verified Successfully"
    });
}

[Authorize(Roles = "Admin")]
[HttpPut("{id}/pickup")]
public async Task<IActionResult> PickupOrder(int id)
{
    var order = await _context.Orders
    .Include(o => o.OrderItems)
    .FirstOrDefaultAsync(o => o.Id == id);

    if (order == null)
        return NotFound();

    if (order.Status != "Verified")
        return BadRequest("Only Verified orders can be picked up");

    foreach (var item in order.OrderItems)
{
    await _httpClient.PutAsync(
        $"https://localhost:7001/api/Inventory/reduce-stock/{item.ProductId}/{item.Quantity}",
        null);
}

var saleDto = new
{
    OrderId = order.Id,
    Amount = order.TotalAmount
};

await _httpClient.PostAsJsonAsync(
    "https://localhost:7001/api/Sales",
    saleDto);



    order.Status = "PickedUp";

    await _context.SaveChangesAsync();

    return Ok(new
    {
        order.Id,
        order.Status,
        Message = "Order Picked Up Successfully"
    });
}
}