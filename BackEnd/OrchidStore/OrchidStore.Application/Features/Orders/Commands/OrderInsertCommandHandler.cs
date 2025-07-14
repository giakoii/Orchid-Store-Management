using System.ComponentModel.DataAnnotations;
using BackEnd.Utils.Const;
using Microsoft.EntityFrameworkCore;
using OrchidStore.Application.CQRS;
using OrchidStore.Application.Logics;
using OrchidStore.Application.Repositories;
using OrchidStore.Application.Utils.Const;
using OrchidStore.Domain.ReadModels;
using OrchidStore.Domain.WriteModels;

namespace OrchidStore.Application.Features.Orders.Commands;

public class OrderInsertCommand : AbstractApiRequest, ICommand<OrderInsertCommandResponse>
{
    public required List<OrderInsertCommandDetail> Items { get; set; }
}

public class OrderInsertCommandDetail
{
    [Required(ErrorMessage = "Orchid ID is required.")]
    public int OrchidId { get; set; }
    
    [Required(ErrorMessage = "Quantity is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
    public int Quantity { get; set; }
}

/// <summary>
/// Command handler for inserting a new order.
/// </summary>
public class OrderInsertCommandHandler : ICommandHandler<OrderInsertCommand, OrderInsertCommandResponse>
{
    private readonly ICommandRepository<Order> _orderRepository;
    private readonly ICommandRepository<OrderDetail> _orderDetailRepository;
    private readonly ICommandRepository<Orchid> _orchidRepository;
    private readonly IIdentityService _identityService;
    private readonly IPaymentLogic _paymentLogic;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="orderRepository"></param>
    /// <param name="orderDetailRepository"></param>
    /// <param name="identityService"></param>
    /// <param name="orchidRepository"></param>
    /// <param name="paymentLogic"></param>
    public OrderInsertCommandHandler(ICommandRepository<Order> orderRepository, ICommandRepository<OrderDetail> orderDetailRepository, IIdentityService identityService, 
        ICommandRepository<Orchid> orchidRepository, IPaymentLogic paymentLogic)
    {
        _orderRepository = orderRepository;
        _orderDetailRepository = orderDetailRepository;
        _identityService = identityService;
        _orchidRepository = orchidRepository;
        _paymentLogic = paymentLogic;
    }

    /// <summary>
    /// Handles the order insertion command.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<OrderInsertCommandResponse> Handle(OrderInsertCommand request, CancellationToken cancellationToken)
    {
        var response = new OrderInsertCommandResponse { Success = false };
        
        // Get current user
        var currentUser = _identityService.GetCurrentUser();
        var currentDate = DateTime.UtcNow;
        
        // Begin transaction
        await _orderRepository.ExecuteInTransactionAsync(async () =>
        {
            // Insert new order
            var newOrder = new Order
            {
                AccountId = int.Parse(currentUser.UserId),
                OrderDate = currentDate,
                OrderStatus = ConstantEnum.OrderStatus.Processing.ToString(),
            };
            
            await _orderRepository.AddAsync(newOrder);
            await _orderRepository.SaveChangesAsync(currentUser.Email);
            
            var orderDetails = new List<OrderDetail>();
            decimal totalAmount = 0;
            foreach (var item in request.Items)
            {
                // Validate OrchidId and Quantity
                var orchid = await _orchidRepository.Find(x => x.OrchidId == item.OrchidId && x.IsActive).FirstOrDefaultAsync();
                if (orchid == null)
                {
                    response.SetMessage(MessageId.I00000, $"Orchid with ID {item.OrchidId} not found or inactive.");
                    return false;
                }
                
                // Insert order detail
                var detail = new OrderDetail
                {
                    OrderId = newOrder.Id,
                    OrchidId = item.OrchidId,
                    Quantity = item.Quantity,
                    Price = item.Quantity * orchid.Price,
                };
                totalAmount += detail.Price;
                orderDetails.Add(detail);
            }
            // Update total amount in order
            newOrder.TotalAmount = totalAmount;

            var paymentRequest = new MomoPaymentRequest
            {
                OrderId = newOrder.Id.ToString(),
                Amount = totalAmount.ToString("0.##"),
                OrderInfo = $"Thanh toán cho đơn hàng {newOrder.Id}",
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
            
            await _orderDetailRepository.AddRangeAsync(orderDetails);
            await _orderDetailRepository.SaveChangesAsync(currentUser.Email);

            // Session save changes
            _orderRepository.Store(OrderCollection.FromWriteModel(newOrder, true), currentUser.Email);
            foreach (var detail in orderDetails)
            {
                _orderDetailRepository.Store(OrderDetailCollection.FromWriteModel(detail, true), currentUser.Email);
            }
            
            await _orderRepository.SessionSavechanges();

            // True
            response.Success = true;
            response.Response = momoResponse;
            response.SetMessage(MessageId.I00001);
            return true;
        });
        return response;
    }
}

public class OrderInsertCommandResponse : AbstractApiResponse<MomoResponse>
{
    public override MomoResponse Response { get; set; }
}

public class MomoResponse
{
    public string? PaymentUrl { get; set; }
    public string? QrCodeUrl { get; set; }
    public string? Deeplink { get; set; }
    public string? DeeplinkWebInApp { get; set; }
}