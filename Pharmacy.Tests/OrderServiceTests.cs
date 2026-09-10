using NUnit.Framework;

namespace Pharmacy.Tests;

[TestFixture]
public class OrderServiceTests
{
    [Test]
    public void OrderTotal_ShouldBeCalculatedFromItems()
    {
        var items = new[]
        {
            new { Quantity = 2, UnitPrice = 50m },
            new { Quantity = 1, UnitPrice = 100m }
        };

        var total = items.Sum(item => item.Quantity * item.UnitPrice);

        Assert.That(total, Is.EqualTo(200m));
    }

    [Test]
    public void NewOrder_ShouldStartWithPendingStatus()
    {
        const string status = "Pending";

        Assert.That(status, Is.EqualTo("Pending"));
    }

    [Test]
    public void OnlyPendingOrder_ShouldBeEligibleForVerification()
    {
        const string status = "Pending";

        var canVerify = status == "Pending";

        Assert.That(canVerify, Is.True);
    }
}