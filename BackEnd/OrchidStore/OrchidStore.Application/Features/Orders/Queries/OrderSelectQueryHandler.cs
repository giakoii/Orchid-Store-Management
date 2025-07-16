using System.ComponentModel.DataAnnotations;
using BackEnd.Utils.Const;
using OrchidStore.Application.CQRS;
using OrchidStore.Application.Logics;
using OrchidStore.Application.Repositories;
using OrchidStore.Domain.ReadModels;

namespace OrchidStore.Application.Features.Orders.Queries;

public class OrderSelectQuery : AbstractApiRequest, IQuery<OrderSelectResponse>
{
    [Required(ErrorMessage = "Order ID is required.")]
    public int OrderId { get; set; }
}

/// <summary>
/// Query handler for selecting an order by ID.
/// </summary>
public class OrderSelectQueryHandler : IQueryHandler<OrderSelectQuery, OrderSelectResponse>
{
    private readonly IQueryRepository<OrderCollection> _orderRepository;
    private readonly IQueryRepository<OrderDetailCollection> _orderDetailRepository;
    private readonly IIdentityService _identityService;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="orderRepository"></param>
    /// <param name="orderDetailRepository"></param>
    /// <param name="identityService"></param>
    public OrderSelectQueryHandler(IQueryRepository<OrderCollection> orderRepository, IQueryRepository<OrderDetailCollection> orderDetailRepository, IIdentityService identityService)
    {
        _orderRepository = orderRepository;
        _orderDetailRepository = orderDetailRepository;
        _identityService = identityService;
    }

    /// <summary>
    /// Handles the select order query by ID.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<OrderSelectResponse> Handle(OrderSelectQuery request, CancellationToken cancellationToken)
    {
        var response = new OrderSelectResponse { Success = false };
        
        var currentUserId = _identityService.GetCurrentUser().UserId;
        
        var order = await _orderRepository.FindOneAsync(x => x.Id == request.OrderId && x.IsActive);
        if (order == null)
        {
            response.SetMessage(MessageId.I00000, "Order not found.");
            return response;
        }

        if (order.AccountId != int.Parse(currentUserId))
        {
            response.SetMessage(MessageId.I00000, "You do not have permission to view this order.");
            return response;
        }
        
        var orderDetails = await _orderDetailRepository.FindAllAsync(x => x.OrderId == order.Id && x.IsActive);
        
        // Set response data
        response.Response = new SelectOrderEntity
        {
            Id = order.Id,
            AccountId = order.AccountId,
            OrderDate = order.OrderDate,
            OrderStatus = order.OrderStatus,
            TotalAmount = order.TotalAmount,
            OrderDetails = orderDetails.Select(detail => new OrderDetail
            {
                OrchidId = detail.Orchid.OrchidId,
                OrchidName = detail.Orchid.OrchidName,
                Price = detail.Price,
                Quantity = detail.Quantity
            }).ToList()
        };
        
        // True
        response.Success = true;
        response.SetMessage(MessageId.I00001);
        return response;
    }
}

public class OrderSelectResponse : AbstractApiResponse<SelectOrderEntity>
{
    public override SelectOrderEntity Response { get; set; }
}

public class SelectOrderEntity
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public DateTime? OrderDate { get; set; }

    public string OrderStatus { get; set; } = null!;

    public decimal TotalAmount { get; set; }
    
    public List<OrderDetail> OrderDetails { get; set; }
}

public class OrderDetail
{
    public int OrchidId { get; set; }
    
    public string OrchidName { get; set; } = null!;

    public decimal Price { get; set; }

    public int Quantity { get; set; }
}