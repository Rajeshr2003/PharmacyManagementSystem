using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Pharmacy.AuthService.Middleware;
using Pharmacy.InventoryService.Middleware;
using Pharmacy.OrderService.Middleware;

namespace Pharmacy.Tests;

[TestFixture]
public class GlobalExceptionMiddlewareTests
{
    [Test]
    public async Task AuthMiddleware_ShouldReturnInternalServerErrorResponse()
    {
        var context = new DefaultHttpContext();

        RequestDelegate next = _ =>
            throw new InvalidOperationException("test exception");

        var middleware = new Pharmacy.AuthService.Middleware.GlobalExceptionMiddleware(
            next,
            NullLogger<Pharmacy.AuthService.Middleware.GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.That(
            context.Response.StatusCode,
            Is.EqualTo(StatusCodes.Status500InternalServerError));
        Assert.That(
            context.Response.ContentType,
            Does.Contain("application/json"));
    }

    [Test]
    public async Task InventoryMiddleware_ShouldReturnInternalServerErrorResponse()
    {
        var context = new DefaultHttpContext();

        RequestDelegate next = _ =>
            throw new InvalidOperationException("test exception");

        var middleware = new Pharmacy.InventoryService.Middleware.GlobalExceptionMiddleware(
            next,
            NullLogger<Pharmacy.InventoryService.Middleware.GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.That(
            context.Response.StatusCode,
            Is.EqualTo(StatusCodes.Status500InternalServerError));
        Assert.That(
            context.Response.ContentType,
            Does.Contain("application/json"));
    }

    [Test]
    public async Task OrderMiddleware_ShouldReturnInternalServerErrorResponse()
    {
        var context = new DefaultHttpContext();

        RequestDelegate next = _ =>
            throw new InvalidOperationException("test exception");

        var middleware = new Pharmacy.OrderService.Middleware.GlobalExceptionMiddleware(
            next,
            NullLogger<Pharmacy.OrderService.Middleware.GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.That(
            context.Response.StatusCode,
            Is.EqualTo(StatusCodes.Status500InternalServerError));
        Assert.That(
            context.Response.ContentType,
            Does.Contain("application/json"));
    }
}