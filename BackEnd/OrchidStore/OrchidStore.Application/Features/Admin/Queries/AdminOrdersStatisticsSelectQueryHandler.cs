using BackEnd.Utils.Const;
using OrchidStore.Application.CQRS;
using OrchidStore.Application.Repositories;
using OrchidStore.Domain.ReadModels;

namespace OrchidStore.Application.Features.Admin.Queries;

public class AdminOrdersStatisticsSelectQuery : AbstractApiRequest, IQuery<AdminOrdersStatisticsSelectQueryResponse>
{
    /// <summary>
    /// Start date for statistics calculation (optional)
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date for statistics calculation (optional)
    /// </summary>
    public DateTime? EndDate { get; set; }
}

public class AdminOrdersStatisticsSelectQueryResponse : AbstractApiResponse<AdminOrdersStatisticsEntity>
{
    public override AdminOrdersStatisticsEntity Response { get; set; }
}

public class AdminOrdersStatisticsEntity
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingOrders { get; set; }
    public int ProcessingOrders { get; set; }
    public int ShippedOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<DailyOrderStatistic> DailyStatistics { get; set; } = new();
    public List<StatusStatistic> StatusStatistics { get; set; } = new();
    public DateRangeEntity DateRange { get; set; }
}

public class DailyOrderStatistic
{
    public DateTime Date { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
}

public class StatusStatistic
{
    public string Status { get; set; } = null!;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Query handler for selecting orders statistics for admin.
/// </summary>
public class AdminOrdersStatisticsSelectQueryHandler : IQueryHandler<AdminOrdersStatisticsSelectQuery, AdminOrdersStatisticsSelectQueryResponse>
{
    private readonly IQueryRepository<OrderCollection> _orderRepository;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="orderRepository"></param>
    public AdminOrdersStatisticsSelectQueryHandler(IQueryRepository<OrderCollection> orderRepository)
    {
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// Handles the admin orders statistics select query.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<AdminOrdersStatisticsSelectQueryResponse> Handle(AdminOrdersStatisticsSelectQuery request, CancellationToken cancellationToken)
    {
        var response = new AdminOrdersStatisticsSelectQueryResponse { Success = false };

        // Set default date range if not provided
        var startDate = request.StartDate ?? DateTime.Now.AddMonths(-1);
        var endDate = request.EndDate ?? DateTime.Now;

        // Get all orders within date range
        var ordersInRange = await _orderRepository.FindAllAsync(x => 
            x.IsActive && 
            x.OrderDate >= startDate && 
            x.OrderDate <= endDate);

        if (!ordersInRange.Any())
        {
            response.SetMessage(MessageId.I00000, "No orders found in the specified date range.");
            return response;
        }

        // Calculate basic statistics
        var totalOrders = ordersInRange.Count();
        var totalRevenue = ordersInRange.Sum(x => x.TotalAmount);
        var averageOrderValue = totalRevenue / totalOrders;

        // Calculate status statistics
        var pendingOrders = ordersInRange.Count(x => x.OrderStatus == "Pending");
        var processingOrders = ordersInRange.Count(x => x.OrderStatus == "Processing");
        var shippedOrders = ordersInRange.Count(x => x.OrderStatus == "Shipped");
        var deliveredOrders = ordersInRange.Count(x => x.OrderStatus == "Delivered");
        var cancelledOrders = ordersInRange.Count(x => x.OrderStatus == "Cancelled");

        // Calculate daily statistics
        var dailyStats = ordersInRange
            .Where(x => x.OrderDate.HasValue)
            .GroupBy(x => x.OrderDate.Value.Date)
            .Select(g => new DailyOrderStatistic
            {
                Date = g.Key,
                OrderCount = g.Count(),
                Revenue = g.Sum(x => x.TotalAmount)
            })
            .OrderBy(x => x.Date)
            .ToList();

        // Calculate status percentage statistics
        var statusStats = new List<StatusStatistic>
        {
            new StatusStatistic { Status = "Pending", Count = pendingOrders, Percentage = (decimal)pendingOrders / totalOrders * 100 },
            new StatusStatistic { Status = "Processing", Count = processingOrders, Percentage = (decimal)processingOrders / totalOrders * 100 },
            new StatusStatistic { Status = "Shipped", Count = shippedOrders, Percentage = (decimal)shippedOrders / totalOrders * 100 },
            new StatusStatistic { Status = "Delivered", Count = deliveredOrders, Percentage = (decimal)deliveredOrders / totalOrders * 100 },
            new StatusStatistic { Status = "Cancelled", Count = cancelledOrders, Percentage = (decimal)cancelledOrders / totalOrders * 100 }
        };

        response.Response = new AdminOrdersStatisticsEntity
        {
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            PendingOrders = pendingOrders,
            ProcessingOrders = processingOrders,
            ShippedOrders = shippedOrders,
            DeliveredOrders = deliveredOrders,
            CancelledOrders = cancelledOrders,
            AverageOrderValue = averageOrderValue,
            DailyStatistics = dailyStats,
            StatusStatistics = statusStats,
            DateRange = new DateRangeEntity
            {
                StartDate = startDate,
                EndDate = endDate
            }
        };

        response.Success = true;
        response.SetMessage(MessageId.I00001);
        return response;
    }
}
