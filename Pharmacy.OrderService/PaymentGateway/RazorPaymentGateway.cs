using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Pharmacy.OrderService.PaymentGateway;

public class RazorpayPaymentGateway : IPaymentGateway
{
    private readonly HttpClient _httpClient;
    private readonly RazorpaySettings _settings;

    public RazorpayPaymentGateway(
        HttpClient httpClient,
        IOptions<RazorpaySettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<PaymentResult> CreatePaymentAsync(
        decimal amount,
        int orderId)
    {
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(
                $"{_settings.KeyId}:{_settings.KeySecret}"));

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);

        var content = JsonContent.Create(new
        {
            amount = (int)Math.Round(amount * 100),
            currency = "INR",
            receipt = $"order_{orderId}"
        });

        var response = await _httpClient.PostAsync(
            "https://api.razorpay.com/v1/orders",
            content);

        if (!response.IsSuccessStatusCode)
        {
            return new PaymentResult(
                false,
                null,
                null,
                "Razorpay payment order could not be created");
        }

        using var document =
            JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        var root = document.RootElement;

        return new PaymentResult(
            true,
            root.GetProperty("id").GetString(),
            null,
            "Razorpay payment order created");
    }

    public Task<PaymentResult> VerifyPaymentAsync(
        string paymentOrderId,
        string paymentId,
        string signature)
    {
        var message = $"{paymentOrderId}|{paymentId}";

        using var hmac = new HMACSHA256(
            Encoding.UTF8.GetBytes(_settings.KeySecret));

        var hash = Convert.ToHexString(
            hmac.ComputeHash(Encoding.UTF8.GetBytes(message)))
            .ToLowerInvariant();

        var valid = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(hash),
            Encoding.UTF8.GetBytes(signature));

        return Task.FromResult(
            new PaymentResult(
                valid,
                paymentOrderId,
                paymentId,
                valid
                    ? "Razorpay payment verified"
                    : "Invalid Razorpay payment signature"));
    }
}