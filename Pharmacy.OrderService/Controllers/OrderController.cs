using System.Security.Claims;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pharmacy.OrderService.Data;
using Pharmacy.OrderService.DTOs;
using Pharmacy.OrderService.Entities;

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

    private void ForwardAuthorizationHeader(HttpRequestMessage request)
    {
        if (Request.Headers.TryGetValue("Authorization", out var authorization))
        {
            request.Headers.TryAddWithoutValidation(
                "Authorization",
                authorization.ToString());
        }
    }

    [Authorize(Roles = "Doctor")]
    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
        {
            return BadRequest("At least one order item is required");
        }

        if (dto.Items.Any(item =>
                item.ProductId <= 0 ||
                item.Quantity <= 0))
        {
            return BadRequest(
                "ProductId must be positive and quantity must be greater than zero");
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("User identity is missing or invalid");
        }

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            Status = "Pending"
        };

        foreach (var item in dto.Items)
        {
            var inventoryRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"http://localhost:5105/api/Inventory/{item.ProductId}");

            ForwardAuthorizationHeader(inventoryRequest);

            var inventoryResponse =
                await _httpClient.SendAsync(inventoryRequest);

            if (inventoryResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound(
                    $"Product {item.ProductId} was not found");
            }

            inventoryResponse.EnsureSuccessStatusCode();

            var inventoryItem =
                await inventoryResponse.Content
                    .ReadFromJsonAsync<InventoryItemResponse>();

            if (inventoryItem is null)
            {
                return Problem(
                    "Inventory Service returned an invalid product response");
            }

            var stockRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"http://localhost:5105/api/Inventory/check/{item.ProductId}/{item.Quantity}");

            ForwardAuthorizationHeader(stockRequest);

            var stockResponse =
                await _httpClient.SendAsync(stockRequest);

            stockResponse.EnsureSuccessStatusCode();

            var available =
                await stockResponse.Content.ReadFromJsonAsync<bool>();

            if (!available)
            {
                return BadRequest(
                    $"Insufficient stock for Product {item.ProductId}");
            }

            order.OrderItems.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = inventoryItem.Price
            });

            order.TotalAmount += item.Quantity * inventoryItem.Price;
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetOrder),
            new { id = order.Id },
            new OrderResponseDto
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
        {
            return NotFound();
        }

        var isAdmin = User.IsInRole("Admin");
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!isAdmin ||
            !int.TryParse(userIdClaim, out var currentUserId))
        {
            if (!isAdmin &&
                (!int.TryParse(userIdClaim, out currentUserId) ||
                 order.UserId != currentUserId))
            {
                return Forbid();
            }
        }

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

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);

        if (order == null)
        {
            return NotFound();
        }

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
        {
            return NotFound();
        }

        if (order.Status != "Pending")
        {
            return Conflict("Only Pending orders can be verified");
        }

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
        {
            return NotFound();
        }

        if (order.Status != "Verified")
        {
            return Conflict("Only Verified orders can be picked up");
        }

        foreach (var item in order.OrderItems)
        {
            var stockRequest = new HttpRequestMessage(
                HttpMethod.Put,
                $"http://localhost:5105/api/Inventory/reduce-stock/{item.ProductId}/{item.Quantity}");

            ForwardAuthorizationHeader(stockRequest);

            var stockResponse =
                await _httpClient.SendAsync(stockRequest);

            stockResponse.EnsureSuccessStatusCode();
        }

        var saleRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "http://localhost:5105/api/Sales");

        ForwardAuthorizationHeader(saleRequest);

        saleRequest.Content = JsonContent.Create(new
        {
            OrderId = order.Id,
            Amount = order.TotalAmount
        });

        var saleResponse =
            await _httpClient.SendAsync(saleRequest);

        saleResponse.EnsureSuccessStatusCode();

        order.Status = "PickedUp";
        await _context.SaveChangesAsync();

        return Ok(new
        {
            order.Id,
            order.Status,
            Message = "Order Picked Up Successfully"
        });
    }

    private sealed class InventoryItemResponse
    {
        public decimal Price { get; set; }
    }
}