using BackEnd.Utils.Const;
using OrchidStore.Application.CQRS;
using OrchidStore.Application.Repositories;
using OrchidStore.Domain.ReadModels;

namespace OrchidStore.Application.Features.Orchids.Queries;

public class SelectOrchidsQuery : AbstractApiRequest, IQuery<SelectOrchidsResponse>
{
    public int? CategoryId { get; set; }
    public bool? IsNatural { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SearchName { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class SelectOrchidsQueryHandler : IQueryHandler<SelectOrchidsQuery, SelectOrchidsResponse>
{
    private readonly IQueryRepository<OrchidCollection> _orchidRepository;

    public SelectOrchidsQueryHandler(IQueryRepository<OrchidCollection> orchidRepository)
    {
        _orchidRepository = orchidRepository;
    }

    public async Task<SelectOrchidsResponse> Handle(SelectOrchidsQuery request, CancellationToken cancellationToken)
    {
        var response = new SelectOrchidsResponse { Success = false };

        // Get all orchids
        var query = await _orchidRepository.FindAllAsync(x => x.IsActive);

        // Apply filters
        if (request.CategoryId.HasValue)
        {
            query = query.Where(x => x.Category.CategoryId == request.CategoryId).ToList();
        }

        if (request.IsNatural.HasValue)
        {
            query = query.Where(x => x.IsNatural == request.IsNatural.Value).ToList();
        }
        
        if (request.MinPrice.HasValue)
        {
            query = query.Where(x => x.Price >= request.MinPrice.Value).ToList();
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(x => x.Price <= request.MaxPrice.Value).ToList();
        }

        if (!string.IsNullOrEmpty(request.SearchName))
        {
            query = query.Where(x => x.OrchidName.Contains(request.SearchName)).ToList();
        }

        // Get total count
        var totalCount = query.Count();

        // Apply pagination
        var orchids = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        response.Response = new SelectOrchidsData
        {
            Orchids = orchids.Select(orchid => new SelectOrchidEntity
            {
                OrchidId = orchid.OrchidId,
                OrchidName = orchid.OrchidName,
                OrchidDescription = orchid.OrchidDescription,
                OrchidUrl = orchid.OrchidUrl,
                Price = orchid.Price,
                IsNatural = orchid.IsNatural,
                CategoryId = orchid.CategoryId,
                CategoryName = orchid.Category?.CategoryName!,
                CreatedAt = orchid.CreatedAt,
            }).ToList(),

            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        // True
        response.Success = true;
        response.SetMessage(MessageId.I00000, "Orchids retrieved successfully.");
        return response;
    }
}