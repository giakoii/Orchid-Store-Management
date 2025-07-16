using BackEnd.Utils.Const;
using OrchidStore.Application.CQRS;
using OrchidStore.Application.Logics;
using OrchidStore.Application.Repositories;
using OrchidStore.Domain.ReadModels;

namespace OrchidStore.Application.Features.Orders.Queries;

public class OrdersSelectQuery : AbstractApiRequest, IQuery<OrdersSelectQueryResponse>
{
    public DateTime? OrderDate { get; set; }
    
    public int PageNumber { get; set; } = 1;
    
    public int PageSize { get; set; } = 10;
}

public class OrdersSelectQueryHandler : IQueryHandler<OrdersSelectQuery, OrdersSelectQueryResponse>
{
    private readonly IQueryRepository<OrderCollection> _orderRepository;
    private readonly IIdentityService _identityService;

    public OrdersSelectQueryHandler(IQueryRepository<OrderCollection> orderRepository, IIdentityService identityService)
    {
        _orderRepository = orderRepository;
        _identityService = identityService;
    }

    public async Task<OrdersSelectQueryResponse> Handle(OrdersSelectQuery request, CancellationToken cancellationToken)
    {
        var respoonse = new OrdersSelectQueryResponse { Success = false };
        
        // Get userId
        var currentUserId = _identityService.GetCurrentUser().UserId;

        // Select all orders
        var orderSelects = await _orderRepository.FindAllAsync(x => x.IsActive && x.AccountId == int.Parse(currentUserId));
        if (!orderSelects.Any())
        {
            respoonse.SetMessage(MessageId.I00000, "Orders is empty.");
            return respoonse;
        }
        
        // Apply filters
        if (request.OrderDate.HasValue)
        {
            orderSelects = orderSelects.Where(x => x.OrderDate == request.OrderDate.Value.Date).ToList();
        }
        else
        {
            orderSelects = orderSelects.OrderByDescending(x => x.OrderDate).ToList();
        }
        
        // Get total count
        var totalCount = orderSelects.Count();

        // Apply pagination
        var orchids = orderSelects
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();
        
        // Map to response entities
        respoonse.Response = orchids.Select(x => new OrdersSelectEntity
        {
            Id = x.Id,
            OrderDate = x.OrderDate
        }).ToList();

        // True
        respoonse.Success = true;
        respoonse.SetMessage(MessageId.I00001);
        return respoonse;
    }
}

public class OrdersSelectQueryResponse : AbstractApiResponse<List<OrdersSelectEntity>>
{
    public override List<OrdersSelectEntity> Response { get; set; }
}

public class OrdersSelectEntity
{
    public int Id { get; set; }
    
    public DateTime? OrderDate { get; set; }
}