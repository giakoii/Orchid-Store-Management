using BackEnd.Utils.Const;
using OrchidStore.Application.CQRS;
using OrchidStore.Application.Repositories;
using OrchidStore.Domain.ReadModels;

namespace OrchidStore.Application.Features.Admin.Queries;

public class SelectAdminBestSellingOrchidsQuery : AbstractApiRequest, IQuery<SelectAdminBestSellingOrchidsQueryResponse>
{
    public int TopCount { get; set; } = 10;
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
}


/// <summary>
/// Query handler for selecting best selling orchids for admin.
/// </summary>
public class SelectAdminBestSellingOrchidsQueryHandler : IQueryHandler<SelectAdminBestSellingOrchidsQuery, SelectAdminBestSellingOrchidsQueryResponse>
{
    private readonly IQueryRepository<OrderDetailCollection> _orderDetailRepository;
    private readonly IQueryRepository<OrchidCollection> _orchidRepository;
    private readonly IQueryRepository<CategoryCollection> _categoryRepository;
    private readonly IQueryRepository<OrderCollection> _orderRepository;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="orderDetailRepository"></param>
    /// <param name="orchidRepository"></param>
    /// <param name="categoryRepository"></param>
    /// <param name="orderRepository"></param>
    public SelectAdminBestSellingOrchidsQueryHandler(IQueryRepository<OrderDetailCollection> orderDetailRepository, IQueryRepository<OrchidCollection> orchidRepository,
        IQueryRepository<CategoryCollection> categoryRepository, IQueryRepository<OrderCollection> orderRepository)
    {
        _orderDetailRepository = orderDetailRepository;
        _orchidRepository = orchidRepository;
        _categoryRepository = categoryRepository;
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// Handles the select admin best selling orchids query.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<SelectAdminBestSellingOrchidsQueryResponse> Handle(SelectAdminBestSellingOrchidsQuery request, CancellationToken cancellationToken)
    {
        var response = new SelectAdminBestSellingOrchidsQueryResponse { Success = false };

        // Set default date range if not provided
        var startDate = request.StartDate ?? DateTime.Now.AddMonths(-3);
        var endDate = request.EndDate ?? DateTime.Now;

        // Get orders within date range
        var ordersInRange = await _orderRepository.FindAllAsync(x => 
            x.IsActive && 
            x.OrderDate >= startDate && 
            x.OrderDate <= endDate);
        if (!ordersInRange.Any())
        {
            response.SetMessage(MessageId.I00000, "No delivered orders found in the specified date range.");
            return response;
        }

        var orderIds = ordersInRange.Select(x => x.Id).ToList();

        // Get order details for delivered orders
        var orderDetails = await _orderDetailRepository.FindAllAsync(x => 
            x.IsActive && orderIds.Contains(x.OrderId));
        if (!orderDetails.Any())
        {
            response.SetMessage(MessageId.I00000, "No order details found.");
            return response;
        }

        // Group by orchid and calculate statistics
        var orchidStats = orderDetails
            .GroupBy(x => x.OrchidId)
            .Select(g => new
            {
                OrchidId = g.Key,
                TotalQuantitySold = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.Price * x.Quantity),
                OrderCount = g.Count()
            })
            .OrderByDescending(x => x.TotalQuantitySold)
            .Take(request.TopCount)
            .ToList();

        // Get orchid details
        var orchidIds = orchidStats.Select(x => x.OrchidId).ToList();
        var orchids = await _orchidRepository.FindAllAsync(x => orchidIds.Contains(x.OrchidId));

        // Get category details
        var categoryIds = orchids.Select(x => x.CategoryId).Distinct().ToList();
        var categories = await _categoryRepository.FindAllAsync(x => categoryIds.Contains(x.CategoryId));

        // Map to response entities
        var bestSellingOrchids = orchidStats.Select(stat =>
        {
            var orchid = orchids.FirstOrDefault(o => o.OrchidId == stat.OrchidId);
            var category = categories.FirstOrDefault(c => c.CategoryId == orchid?.CategoryId);
            
            return new BestSellingOrchidEntity
            {
                OrchidId = stat.OrchidId,
                OrchidName = orchid?.OrchidName!,
                CategoryName = category?.CategoryName!,
                Price = orchid!.Price,
                TotalQuantitySold = stat.TotalQuantitySold,
                TotalRevenue = stat.TotalRevenue,
                OrderCount = stat.OrderCount,
                ImageUrl = orchid?.OrchidUrl
            };
        }).ToList();

        response.Response = new SelectAdminBestSellingOrchidsEntity
        {
            BestSellingOrchids = bestSellingOrchids,
            DateRange = new DateRangeEntity
            {
                StartDate = startDate,
                EndDate = endDate
            },
            TotalProductsAnalyzed = orchidStats.Count
        };

        response.Success = true;
        response.SetMessage(MessageId.I00001);
        return response;
    }
}

public class SelectAdminBestSellingOrchidsQueryResponse : AbstractApiResponse<SelectAdminBestSellingOrchidsEntity>
{
    public override SelectAdminBestSellingOrchidsEntity Response { get; set; }
}

public class SelectAdminBestSellingOrchidsEntity
{
    public List<BestSellingOrchidEntity> BestSellingOrchids { get; set; } = new();
    public DateRangeEntity DateRange { get; set; }
    public int TotalProductsAnalyzed { get; set; }
}

public class BestSellingOrchidEntity
{
    public int OrchidId { get; set; }
    public string OrchidName { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public decimal Price { get; set; }
    public int TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
    public string? ImageUrl { get; set; }
}

public class DateRangeEntity
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}