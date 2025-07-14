namespace OrchidStore.Application.Logics;

public interface IPaymentLogic
{
    Task<MomoPaymentResponse> CreatePaymentOrderAsync(MomoPaymentRequest model);
}

public class MomoPaymentResponse
{
    public string RequestId { get; set; }
    public int ErrorCode { get; set; }
    public string OrderId { get; set; }
    public string Message { get; set; }
    public string? PayUrl { get; set; }
    public string? QrCodeUrl { get; set; }
    public string? Deeplink { get; set; }
    public string? DeeplinkWebInApp { get; set; }
}

public class MomoPaymentRequest
{
    public string OrderId { get; set; }
    public string Amount { get; set; }
    public string OrderInfo { get; set; } 
}