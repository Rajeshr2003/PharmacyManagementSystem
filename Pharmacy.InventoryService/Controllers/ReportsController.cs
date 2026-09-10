using Microsoft.AspNetCore.Mvc;
using Pharmacy.InventoryService.Data;
using Microsoft.AspNetCore.Authorization;
namespace Pharmacy.InventoryService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly InventoryDbContext _context;

    public ReportsController(InventoryDbContext context)
    {
        _context = context;
    }

    [HttpGet("sales")]
    public IActionResult GetSalesReport()
    {
        var sales = _context.Sales.ToList();

        var totalSales = sales.Sum(x => x.Amount);

        return Ok(new
        {
            TotalRecords = sales.Count,
            TotalSalesAmount = totalSales,
            Sales = sales
        });
    }
}