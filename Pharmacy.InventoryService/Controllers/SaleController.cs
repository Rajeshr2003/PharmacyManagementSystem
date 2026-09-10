using Microsoft.AspNetCore.Mvc;
using Pharmacy.InventoryService.DTOs;
using Pharmacy.InventoryService.Interfaces;

namespace Pharmacy.InventoryService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly ISalesService _service;

    public SalesController(ISalesService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetSales()
    {
        return Ok(await _service.GetAllSales());
    }

    [HttpPost]
    public async Task<IActionResult> AddSale(
        SaleDto dto)
    {
        return Ok(await _service.AddSale(dto));
    }
}