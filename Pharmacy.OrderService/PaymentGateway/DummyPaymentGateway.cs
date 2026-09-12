namespace Pharmacy.OrderService.PaymentGateway;

public class DummyPaymentGateway : IPaymentGateway
{
    public Task<PaymentResult> CreatePaymentAsync(
        decimal amount,
        int orderId)
    {
        return Task.FromResult(
            new PaymentResult(
                true,
                $"dummy-order-{orderId}",
                null,
                "Dummy payment order created"));
    }

    public Task<PaymentResult> VerifyPaymentAsync(
        string paymentOrderId,
        string paymentId,
        string signature)
    {
        if (string.IsNullOrWhiteSpace(paymentOrderId) ||
            string.IsNullOrWhiteSpace(paymentId) ||
            string.IsNullOrWhiteSpace(signature))
        {
            return Task.FromResult(
                new PaymentResult(
                    false,
                    paymentOrderId,
                    paymentId,
                    "Payment details are required"));
        }

        return Task.FromResult(
            new PaymentResult(
                true,
                paymentOrderId,
                paymentId,
                "Dummy payment verified"));
    }
}