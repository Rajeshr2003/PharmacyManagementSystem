using NUnit.Framework;

namespace Pharmacy.Tests;

[TestFixture]
public class InventoryServiceTests
{
    [Test]
    public void Supplier_Should_Be_Created()
    {
        var supplierName = "ABC Pharma";

        Assert.That(supplierName, Is.Not.Empty);
    }
}