using Microsoft.AspNetCore.Mvc;
using Pharmacy.InventoryService.DTOs;
using Pharmacy.InventoryService.Interfaces;

namespace Pharmacy.InventoryService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SupplierController : ControllerBase
{
    private readonly ISupplierService _service;

    public SupplierController(ISupplierService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAll());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var supplier = await _service.GetById(id);

        if (supplier == null)
            return NotFound();

        return Ok(supplier);
    }

    [HttpPost]
    public async Task<IActionResult> Add(
        SupplierDto dto)
    {
        return Ok(await _service.Add(dto));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        SupplierDto dto)
    {
        var supplier =
            await _service.Update(id, dto);

        if (supplier == null)
            return NotFound();

        return Ok(supplier);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result =
            await _service.Delete(id);

        if (!result)
            return NotFound();

        return NoContent();
    }
}