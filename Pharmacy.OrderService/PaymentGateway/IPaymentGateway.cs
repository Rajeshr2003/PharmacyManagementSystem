namespace Pharmacy.OrderService.PaymentGateway;

public interface IPaymentGateway
{
    Task<PaymentResult> CreatePaymentAsync(
        decimal amount,
        int orderId);

    Task<PaymentResult> VerifyPaymentAsync(
        string paymentOrderId,
        string paymentId,
        string signature);
}

public sealed record PaymentResult(
    bool IsSuccessful,
    string? PaymentOrderId,
    string? PaymentId,
    string Message);