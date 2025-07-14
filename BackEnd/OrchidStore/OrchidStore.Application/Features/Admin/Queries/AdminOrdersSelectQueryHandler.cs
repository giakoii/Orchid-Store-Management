using BackEnd.Utils.Const;
using Microsoft.EntityFrameworkCore;
using OrchidStore.Application.CQRS;
using OrchidStore.Application.Repositories;
using OrchidStore.Domain.ReadModels;

namespace OrchidStore.Application.Features.Admin.Queries;

public class AdminOrdersSelectQuery : AbstractApiRequest, IQuery<AdminOrdersSelectQueryResponse>
{
    /// <summary>
    /// Page number for pagination (default: 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size for pagination (default: 10)
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Filter by order status (optional)
    /// </summary>
    public string? OrderStatus { get; set; }

    /// <summary>
    /// Filter by customer email (optional)
    /// </summary>
    public string? CustomerEmail { get; set; }

    /// <summary>
    /// Filter by order date (optional)
    /// </summary>
    public DateTime? OrderDate { get; set; }
}

public class AdminOrdersSelectQueryResponse : AbstractApiResponse<AdminOrdersSelectEntity>
{
    public override AdminOrdersSelectEntity Response { get; set; }
}

public class AdminOrdersSelectEntity
{
    public List<AdminOrderEntity> Orders { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class AdminOrderEntity
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public DateTime? OrderDate { get; set; }
    public string OrderStatus { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public string CustomerName { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;
}

/// <summary>
/// Query handler for selecting all orders for admin with filters and pagination.
/// </summary>
public class AdminOrdersSelectQueryHandler : IQueryHandler<AdminOrdersSelectQuery, AdminOrdersSelectQueryResponse>
{
    private readonly IQueryRepository<OrderCollection> _orderRepository;
    private readonly IQueryRepository<AccountCollection> _accountRepository;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="orderRepository"></param>
    /// <param name="accountRepository"></param>
    public AdminOrdersSelectQueryHandler(IQueryRepository<OrderCollection> orderRepository, IQueryRepository<AccountCollection> accountRepository)
    {
        _orderRepository = orderRepository;
        _accountRepository = accountRepository;
    }

    /// <summary>
    /// Handles the admin orders select query with filters and pagination.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<AdminOrdersSelectQueryResponse> Handle(AdminOrdersSelectQuery request, CancellationToken cancellationToken)
    {
        var response = new AdminOrdersSelectQueryResponse { Success = false };

        // Select all orders for admin
        var orderSelects = await _orderRepository.FindAllAsync(x => x.IsActive);
        if (!orderSelects.Any())
        {
            response.SetMessage(MessageId.I00000, "Orders is empty.");
            return response;
        }

        // Apply filters
        if (!string.IsNullOrEmpty(request.OrderStatus))
        {
            orderSelects = orderSelects.Where(x => x.OrderStatus == request.OrderStatus).ToList();
        }

        if (request.OrderDate.HasValue)
        {
            orderSelects = orderSelects.Where(x => x.OrderDate?.Date == request.OrderDate.Value.Date).ToList();
        }

        // Apply customer email filter
        if (!string.IsNullOrEmpty(request.CustomerEmail))
        {
            var customerIds = await _accountRepository.FindAllAsync(x => x.IsActive && x.Email.Contains(request.CustomerEmail));
            var accountIds = customerIds.Select(x => x.AccountId).ToList();
            orderSelects = orderSelects.Where(x => accountIds.Contains(x.AccountId)).ToList();
        }

        // Get total count
        var totalCount = orderSelects.Count();

        // Apply pagination
        var orders = orderSelects
            .OrderByDescending(x => x.OrderDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Get customer details for orders
        var orderAccountIds = orders.Select(x => x.AccountId).Distinct().ToList();
        var accounts = await _accountRepository.FindAllAsync(x => orderAccountIds.Contains(x.AccountId));

        // Map to response entities
        var orderEntities = orders.Select(order =>
        {
            var customer = accounts.FirstOrDefault(a => a.AccountId == order.AccountId);
            return new AdminOrderEntity
            {
                Id = order.Id,
                AccountId = order.AccountId,
                OrderDate = order.OrderDate,
                OrderStatus = order.OrderStatus,
                TotalAmount = order.TotalAmount,
                CustomerName = customer?.AccountName ?? "Unknown",
                CustomerEmail = customer?.Email ?? "Unknown"
            };
        }).ToList();

        response.Response = new AdminOrdersSelectEntity
        {
            Orders = orderEntities,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };

        response.Success = true;
        response.SetMessage(MessageId.I00001);
        return response;
    }
}
