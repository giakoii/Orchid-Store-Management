using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrchidStore.Application.Logics;
using RestSharp;

namespace OrchidStore.Infrastructure.Logics;

public class PaymentService : IPaymentLogic
{
    private readonly IOptions<MomoOptionModel> _options;
    
    public PaymentService(IOptions<MomoOptionModel> options)
    {
        _options = options;
    }

    public async Task<MomoPaymentResponse> CreatePaymentOrderAsync(MomoPaymentRequest model)
    {
        model.OrderId = DateTime.UtcNow.Ticks.ToString();
        model.OrderInfo = model.OrderInfo;
        var rawData =
            $"partnerCode={_options.Value.PartnerCode}" +
            $"&accessKey={_options.Value.AccessKey}" +
            $"&requestId={model.OrderId}" +
            $"&amount={model.Amount}" +
            $"&orderId={model.OrderId}" +
            $"&orderInfo={model.OrderInfo}" +
            $"&returnUrl={_options.Value.ReturnUrl}" +
            $"&notifyUrl={_options.Value.NotifyUrl}" +
            $"&extraData=";
        var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);
        var client = new RestClient(_options.Value.MomoApiUrl);
        var request = new RestRequest() { Method = Method.Post };
        request.AddHeader("Content-Type", "application/json; charset=UTF-8");
        
        var requestData = new
        {
            accessKey = _options.Value.AccessKey,
            partnerCode = _options.Value.PartnerCode,
            requestType = _options.Value.RequestType,
            notifyUrl = _options.Value.NotifyUrl,
            returnUrl = _options.Value.ReturnUrl,
            orderId = model.OrderId,
            amount = model.Amount.ToString(),
            orderInfo = model.OrderInfo,
            requestId = model.OrderId,
            extraData = "",
            signature = signature
        };

        request.AddParameter("application/json", JsonConvert.SerializeObject(requestData), 
            ParameterType.RequestBody);

        var response = await client.ExecuteAsync(request);
        var momoResponse = JsonConvert.DeserializeObject<MomoPaymentResponse>(response.Content);
        return momoResponse;
    }
    
    private string ComputeHmacSha256(string message, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        byte[] hashBytes;

        using (var hmac = new HMACSHA256(keyBytes))
        {
            hashBytes = hmac.ComputeHash(messageBytes);
        }

        var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        return hashString;
    }
}

public class MomoOptionModel
{
    public string MomoApiUrl { get; set; }
    public string SecretKey { get; set; }
    public string AccessKey { get; set; }
    public string ReturnUrl { get; set; }
    public string NotifyUrl { get; set; }
    public string PartnerCode { get; set; }
    public string RequestType { get; set; }
}