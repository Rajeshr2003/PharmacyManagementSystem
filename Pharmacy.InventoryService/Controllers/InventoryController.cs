using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.InventoryService.DTOs;
using Pharmacy.InventoryService.Interfaces;

namespace Pharmacy.InventoryService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _service;

    public InventoryController(IInventoryService service)
    {
        _service = service;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> AddMedicine(AddMedicineDto dto)
    {
        var result = await _service.AddMedicine(dto);

        return Ok(result);
    }

    [Authorize(Roles = "Admin,Doctor")]
    [HttpGet]
    public async Task<IActionResult> GetAllMedicines()
    {
        var result = await _service.GetAllMedicines();

        return Ok(result);
    }

    [Authorize(Roles = "Admin,Doctor")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMedicineById(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Id must be greater than zero");
        }

        var result = await _service.GetMedicineById(id);

        if (result == null)
        {
            return NotFound("Medicine was not found");
        }

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMedicine(
        int id,
        UpdateMedicineDto dto)
    {
        if (id <= 0)
        {
            return BadRequest("Id must be greater than zero");
        }

        var result = await _service.UpdateMedicine(id, dto);

        if (result == null)
        {
            return NotFound("Medicine was not found");
        }

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMedicine(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Id must be greater than zero");
        }

        var result = await _service.DeleteMedicine(id);

        if (!result)
        {
            return NotFound("Medicine was not found");
        }

        return NoContent();
    }

    [Authorize(Roles = "Admin,Doctor")]
    [HttpGet("check/{id}/{quantity}")]
    public async Task<IActionResult> CheckStock(int id, int quantity)
    {
        if (id <= 0 || quantity <= 0)
        {
            return BadRequest(
                "Id must be greater than zero and quantity must be positive");
        }

        var medicine = await _service.GetMedicineById(id);

        if (medicine == null)
        {
            return NotFound("Medicine was not found");
        }

        var available = await _service.HasStock(id, quantity);

        return Ok(available);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("reduce-stock/{id}/{quantity}")]
    public async Task<IActionResult> ReduceStock(int id, int quantity)
    {
        if (id <= 0 || quantity <= 0)
        {
            return BadRequest(
                "Id must be greater than zero and quantity must be positive");
        }

        var medicine = await _service.GetMedicineById(id);

        if (medicine == null)
        {
            return NotFound("Medicine was not found");
        }

        if (medicine.StockQuantity < quantity)
        {
            return Conflict("Insufficient stock");
        }

        var result = await _service.ReduceStock(id, quantity);

        if (!result)
        {
            return Conflict("Stock could not be reduced");
        }

        return Ok(new
        {
            Message = "Stock reduced successfully"
        });
    }
}