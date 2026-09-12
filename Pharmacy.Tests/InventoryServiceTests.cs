using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Pharmacy.InventoryService.Data;
using Pharmacy.InventoryService.DTOs;
using InventoryServiceImplementation =
    Pharmacy.InventoryService.Services.InventoryService;

namespace Pharmacy.Tests;

[TestFixture]
public class InventoryServiceTests
{
    [Test]
    public async Task AddMedicine_ShouldSaveMedicine()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase($"InventoryTests-{Guid.NewGuid()}")
            .Options;

        await using var context = new InventoryDbContext(options);
        var service = new InventoryServiceImplementation(context);

        var medicine = await service.AddMedicine(new AddMedicineDto
        {
            MedicineName = "Test Medicine",
            Description = "Tablet",
            Price = 20,
            StockQuantity = 5
        });

        Assert.That(medicine.Id, Is.GreaterThan(0));
        Assert.That(medicine.MedicineName, Is.EqualTo("Test Medicine"));
        Assert.That(medicine.StockQuantity, Is.EqualTo(5));

        var savedMedicine = await context.Inventories
            .SingleAsync(x => x.Id == medicine.Id);

        Assert.That(savedMedicine.Price, Is.EqualTo(20));
    }

    [Test]
    public async Task ReduceStock_ShouldDecreaseAvailableQuantity()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase($"InventoryTests-{Guid.NewGuid()}")
            .Options;

        await using var context = new InventoryDbContext(options);
        var service = new InventoryServiceImplementation(context);

        var medicine = await service.AddMedicine(new AddMedicineDto
        {
            MedicineName = "Paracetamol",
            Description = "Tablet",
            Price = 20,
            StockQuantity = 5
        });

        var result = await service.ReduceStock(medicine.Id, 2);

        Assert.That(result, Is.True);

        var updatedMedicine = await context.Inventories
            .SingleAsync(x => x.Id == medicine.Id);

        Assert.That(updatedMedicine.StockQuantity, Is.EqualTo(3));
    }

    [Test]
    public async Task ReduceStock_ShouldReturnFalse_WhenQuantityIsInsufficient()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase($"InventoryTests-{Guid.NewGuid()}")
            .Options;

        await using var context = new InventoryDbContext(options);
        var service = new InventoryServiceImplementation(context);

        var medicine = await service.AddMedicine(new AddMedicineDto
        {
            MedicineName = "Paracetamol",
            Description = "Tablet",
            Price = 20,
            StockQuantity = 2
        });

        var result = await service.ReduceStock(medicine.Id, 5);

        Assert.That(result, Is.False);
    }
}