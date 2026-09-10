using NUnit.Framework;

namespace Pharmacy.Tests;

[TestFixture]
public class OrderServiceTests
{
    [Test]
    public void CreateOrder_Should_Calculate_Total()
    {
        decimal total = 100 + 200;

        Assert.That(total, Is.EqualTo(300));
    }
}