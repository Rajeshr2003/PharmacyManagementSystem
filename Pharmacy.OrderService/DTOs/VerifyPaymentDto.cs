namespace Pharmacy.OrderService.DTOs;

public class VerifyPaymentDto
{
    public int OrderId { get; set; }

    public string PaymentOrderId { get; set; } = string.Empty;

    public string PaymentId { get; set; } = string.Empty;

    public string Signature { get; set; } = string.Empty;
}