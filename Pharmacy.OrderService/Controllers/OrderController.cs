using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pharmacy.OrderService.Data;
using Pharmacy.OrderService.DTOs;
using Pharmacy.OrderService.Entities;
using Pharmacy.OrderService.PaymentGateway;

namespace Pharmacy.OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly OrdersDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IPaymentGateway _paymentGateway;

    public OrderController(
        OrdersDbContext context,
        IHttpClientFactory httpClientFactory,
        IPaymentGateway paymentGateway)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient();
        _paymentGateway = paymentGateway;
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
            using var inventoryRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"http://localhost:5105/api/Inventory/{item.ProductId}");

            ForwardAuthorizationHeader(inventoryRequest);

            var inventoryResponse =
                await _httpClient.SendAsync(inventoryRequest);

            if (inventoryResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(
                    $"Product {item.ProductId} was not found");
            }

            if (!inventoryResponse.IsSuccessStatusCode)
            {
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    "Inventory Service could not be reached");
            }

            var inventoryItem =
                await inventoryResponse.Content
                    .ReadFromJsonAsync<InventoryItemResponse>();

            if (inventoryItem is null)
            {
                return Problem(
                    "Inventory Service returned an invalid product response");
            }

            using var stockRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"http://localhost:5105/api/Inventory/check/{item.ProductId}/{item.Quantity}");

            ForwardAuthorizationHeader(stockRequest);

            var stockResponse =
                await _httpClient.SendAsync(stockRequest);

            if (!stockResponse.IsSuccessStatusCode)
            {
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    "Inventory stock could not be checked");
            }

            var available =
                await stockResponse.Content.ReadFromJsonAsync<bool>();

            if (!available)
            {
                return Conflict(
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

    [Authorize(Roles = "Doctor")]
    [HttpPost("payment")]
    public async Task<IActionResult> CreatePayment(
        CreatePaymentDto dto)
    {
        if (dto.OrderId <= 0)
        {
            return BadRequest("OrderId must be greater than zero");
        }

        var order = await _context.Orders
            .FirstOrDefaultAsync(item => item.Id == dto.OrderId);

        if (order == null)
        {
            return NotFound("Order was not found");
        }

        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("User identity is missing or invalid");
        }

        if (order.UserId != userId)
        {
            return Forbid();
        }

        if (order.Status != "Pending")
        {
            return Conflict(
                "Payment is not allowed for this order");
        }

        var result = await _paymentGateway.CreatePaymentAsync(
            order.TotalAmount,
            order.Id);

        if (!result.IsSuccessful)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                result.Message);
        }

        return Ok(result);
    }

    [Authorize(Roles = "Doctor")]
    [HttpPost("payment/verify")]
    public async Task<IActionResult> VerifyPayment(
        VerifyPaymentDto dto)
    {
        if (dto.OrderId <= 0)
        {
            return BadRequest("OrderId must be greater than zero");
        }

        if (string.IsNullOrWhiteSpace(dto.PaymentOrderId) ||
            string.IsNullOrWhiteSpace(dto.PaymentId) ||
            string.IsNullOrWhiteSpace(dto.Signature))
        {
            return BadRequest("Payment details are required");
        }

        var order = await _context.Orders
            .FirstOrDefaultAsync(item => item.Id == dto.OrderId);

        if (order == null)
        {
            return NotFound("Order was not found");
        }

        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("User identity is missing or invalid");
        }

        if (order.UserId != userId)
        {
            return Forbid();
        }

        if (order.Status != "Pending")
        {
            return Conflict(
                "Payment has already been processed");
        }

        var result = await _paymentGateway.VerifyPaymentAsync(
            dto.PaymentOrderId,
            dto.PaymentId,
            dto.Signature);

        if (!result.IsSuccessful)
        {
            return BadRequest(result.Message);
        }

        order.Status = "Paid";
        await _context.SaveChangesAsync();

        return Ok(new
        {
            order.Id,
            order.Status,
            result.Message
        });
    }

    [Authorize(Roles = "Admin,Doctor")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Id must be greater than zero");
        }

        var order = await _context.Orders
            .Include(item => item.OrderItems)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (order == null)
        {
            return NotFound("Order was not found");
        }

        var isAdmin = User.IsInRole("Admin");

        if (!isAdmin)
        {
            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var currentUserId) ||
                order.UserId != currentUserId)
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
            .Include(item => item.OrderItems)
            .ToListAsync();

        return Ok(orders);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Id must be greater than zero");
        }

        var order = await _context.Orders.FindAsync(id);

        if (order == null)
        {
            return NotFound("Order was not found");
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/verify")]
    public async Task<IActionResult> VerifyOrder(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Id must be greater than zero");
        }

        var order = await _context.Orders.FindAsync(id);

        if (order == null)
        {
            return NotFound("Order was not found");
        }

        if (order.Status != "Pending" &&
            order.Status != "Paid")
        {
            return Conflict(
                "Only Pending or Paid orders can be verified");
        }

        order.Status = "Verified";
        await _context.SaveChangesAsync();

        return Ok(new
        {
            order.Id,
            order.Status,
            Message = "Order verified successfully"
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/pickup")]
    public async Task<IActionResult> PickupOrder(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Id must be greater than zero");
        }

        var order = await _context.Orders
            .Include(item => item.OrderItems)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (order == null)
        {
            return NotFound("Order was not found");
        }

        if (order.Status != "Verified")
        {
            return Conflict(
                "Only Verified orders can be picked up");
        }

        foreach (var item in order.OrderItems)
        {
            using var stockRequest = new HttpRequestMessage(
                HttpMethod.Put,
                $"http://localhost:5105/api/Inventory/reduce-stock/{item.ProductId}/{item.Quantity}");

            ForwardAuthorizationHeader(stockRequest);

            var stockResponse =
                await _httpClient.SendAsync(stockRequest);

            if (!stockResponse.IsSuccessStatusCode)
            {
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    "Inventory stock could not be reduced");
            }
        }

        using var saleRequest = new HttpRequestMessage(
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

        if (!saleResponse.IsSuccessStatusCode)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                "Sale could not be created");
        }

        order.Status = "PickedUp";
        await _context.SaveChangesAsync();

        return Ok(new
        {
            order.Id,
            order.Status,
            Message = "Order picked up successfully"
        });
    }

    private sealed class InventoryItemResponse
    {
        public decimal Price { get; set; }
    }
}