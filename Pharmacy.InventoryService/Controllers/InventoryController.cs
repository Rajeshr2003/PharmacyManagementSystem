using Microsoft.AspNetCore.Mvc;
using Pharmacy.InventoryService.DTOs;
using Pharmacy.InventoryService.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
        var result = await _service.GetMedicineById(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMedicine(
        int id,
        UpdateMedicineDto dto)
    {
        var result = await _service.UpdateMedicine(id, dto);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMedicine(int id)
    {
        var result = await _service.DeleteMedicine(id);

        if (!result)
            return NotFound();

        return NoContent();
    }

    [Authorize(Roles = "Admin,Doctor")]
    [HttpGet("check/{id}/{quantity}")]
    public async Task<IActionResult> CheckStock(
    int id,
    int quantity)
    {
        var available =
            await _service.HasStock(id, quantity);

        return Ok(available);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("reduce-stock/{id}/{quantity}")]
    public async Task<IActionResult> ReduceStock(
    int id,
    int quantity)
    {
        var result =
            await _service.ReduceStock(id, quantity);

        if (!result)
            return BadRequest();

        return Ok();
    }
}