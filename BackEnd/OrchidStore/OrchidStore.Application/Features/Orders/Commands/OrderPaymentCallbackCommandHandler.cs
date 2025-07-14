using System.ComponentModel.DataAnnotations;
using BackEnd.Utils.Const;
using OrchidStore.Application.CQRS;
using OrchidStore.Application.Logics;
using OrchidStore.Application.Repositories;
using OrchidStore.Application.Utils.Const;
using OrchidStore.Domain.ReadModels;
using OrchidStore.Domain.WriteModels;

namespace OrchidStore.Application.Features.Orders.Commands;

public class OrderPaymentCallbackCommand : AbstractApiRequest, ICommand<OrderInsertCommandResponse>
{
    [Required(ErrorMessage = "Order Id is required")]
    public string OrderInfo { get; set; } = null!;
    
    [Required(ErrorMessage = "Error code is required")]
    public string ErrorCode { get; set; } = null!;
}

/// <summary>
/// Command handler for processing order payment callbacks.
/// </summary>
public class OrderPaymentCallbackCommandHandler : ICommandHandler<OrderPaymentCallbackCommand, OrderInsertCommandResponse>
{
    private readonly ICommandRepository<Order> _orderRepository;
    private readonly IPaymentLogic _paymentLogic;
    private readonly IIdentityService _identityService;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="orderRepository"></param>
    /// <param name="identityService"></param>
    /// <param name="paymentLogic"></param>
    public OrderPaymentCallbackCommandHandler(ICommandRepository<Order> orderRepository,
        IIdentityService identityService, IPaymentLogic paymentLogic)
    {
        _orderRepository = orderRepository;
        _identityService = identityService;
        _paymentLogic = paymentLogic;
    }

    /// <summary>
    /// Handles the order payment callback command.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<OrderInsertCommandResponse> Handle(OrderPaymentCallbackCommand request, CancellationToken cancellationToken)
    {
        var response = new OrderInsertCommandResponse { Success = false };
        
        var currentEmail = _identityService.GetCurrentUser().Email;

        var orderId = ExtractOrderIdFromOrderInfo(request.OrderInfo);
        if (string.IsNullOrWhiteSpace(orderId))
        {
            response.SetMessage(MessageId.I00000, "Order ID is invalid.");
            return response;
        }

        // Validate OrderId
        var order = _orderRepository.Find(x => x.Id == int.Parse(orderId)).FirstOrDefault();
        if (order == null)
        {
            response.SetMessage(MessageId.I00000, "Order not found.");
            return response;
        }

        // Check if the order is already paid
        if (order.OrderStatus == ConstantEnum.OrderStatus.Completed.ToString() ||
            order.OrderStatus == ConstantEnum.OrderStatus.Cancelled.ToString())
        {
            response.SetMessage(MessageId.I00000, "Order is already paid.");
            return response;
        }

        // Begin transaction
        await _orderRepository.ExecuteInTransactionAsync(async () =>
        {
            // Validate error code
            if (request.ErrorCode !=((byte) ConstantEnum.PaymentStatus.Success).ToString())
            {
                var paymentRequest = new MomoPaymentRequest
                {
                    OrderId = order.Id.ToString(),
                    Amount = order.TotalAmount.ToString("0.##"),
                    OrderInfo = $"Thanh toán cho đơn hàng {order.Id}",
                };
            
                // Process payment
                var paymentResult = await _paymentLogic.CreatePaymentOrderAsync(paymentRequest);
                if (paymentResult.ErrorCode != (byte) ConstantEnum.PaymentStatus.Success || 
                    paymentResult.PayUrl == null ||
                    paymentResult.QrCodeUrl == null ||
                    paymentResult.DeeplinkWebInApp == null ||
                    paymentResult.Deeplink == null)
                {
                    response.SetMessage(MessageId.I00000, CommonMessages.PaymentFailed);
                    return false;
                }
            
                var momoResponse = new MomoResponse
                {
                    PaymentUrl = paymentResult.PayUrl,
                    QrCodeUrl = paymentResult.QrCodeUrl,
                    DeeplinkWebInApp = paymentResult.DeeplinkWebInApp,
                    Deeplink = paymentResult.Deeplink,
                };
                
                response.Response = momoResponse;
                response.SetMessage(MessageId.I00000, "Payment is being processed. Please try again later.");
                return false;
            }
            
            order.OrderStatus = ConstantEnum.OrderStatus.Completed.ToString();
            
            // Save changes
            _orderRepository.Update(order);
            await _orderRepository.SaveChangesAsync(currentEmail);
            
            // Store the order in the collection
            _orderRepository.Store(OrderCollection.FromWriteModel(order), currentEmail, true);
            await _orderRepository.SessionSavechanges();

            // True
            response.Success = true;
            response.SetMessage(MessageId.I00001);
            return true;
        });
        return response;
    }

    private string ExtractOrderIdFromOrderInfo(string orderInfo)
    {
        // "Thanh toán cho đơn hàng {OrderId}"
        if (string.IsNullOrEmpty(orderInfo))
            return string.Empty;

        var startIndex = orderInfo.LastIndexOf(' ') + 1;
        if (startIndex > 0 && startIndex < orderInfo.Length)
        {
            return orderInfo.Substring(startIndex);
        }

        return string.Empty;
    }
}