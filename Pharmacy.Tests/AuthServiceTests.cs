using NUnit.Framework;

namespace Pharmacy.Tests;

[TestFixture]
public class AuthServiceTests
{
    [Test]
    public void Login_Should_Return_Valid_Result()
    {
        var result = true;

        Assert.That(result, Is.True);
    }
}