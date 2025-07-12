using System.ComponentModel.DataAnnotations;
using BackEnd.Utils.Const;
using OrchidStore.Application.CQRS;
using OrchidStore.Application.Features;
using OrchidStore.Application.Repositories;
using OrchidStore.Domain.ReadModels;

namespace OrchidStore.Application.Features.Orchids.Queries;

// SelectOrchidQuery cho single orchid
public class SelectOrchidQuery : AbstractApiRequest, IQuery<SelectOrchidResponse>
{
    [Required(ErrorMessage = "Orchid ID is required.")]
    public int OrchidId { get; set; }
}

public class SelectOrchidResponse : AbstractApiResponse<SelectOrchidEntity>
{
    public override SelectOrchidEntity Response { get; set; }
}

// SelectOrchidsQuery cho multiple orchids
public class SelectOrchidsQuery : AbstractApiRequest, IQuery<SelectOrchidsResponse>
{
    public int? CategoryId { get; set; }
    public bool? IsNatural { get; set; }
    public bool? IsActive { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SearchName { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class SelectOrchidsResponse : AbstractApiResponse<SelectOrchidsData>
{
    public override SelectOrchidsData Response { get; set; }
}

public class SelectOrchidsData
{
    public List<SelectOrchidEntity> Orchids { get; set; } = new List<SelectOrchidEntity>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class SelectOrchidEntity
{
    public int OrchidId { get; set; }
    public string OrchidName { get; set; } = null!;
    public string? OrchidDescription { get; set; }
    public string? OrchidUrl { get; set; }
    public decimal Price { get; set; }
    public bool IsNatural { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = null!;
}

// Handler cho single orchid
public class SelectOrchidQueryHandler : IQueryHandler<SelectOrchidQuery, SelectOrchidResponse>
{
    private readonly IQueryRepository<OrchidCollection> _orchidRepository;

    public SelectOrchidQueryHandler(IQueryRepository<OrchidCollection> orchidRepository)
    {
        _orchidRepository = orchidRepository;
    }

    public async Task<SelectOrchidResponse> Handle(SelectOrchidQuery request, CancellationToken cancellationToken)
    {
        var response = new SelectOrchidResponse { Success = false };

        try
        {
            var orchid = await _orchidRepository.FindOneAsync(x => x.OrchidId == request.OrchidId);

            if (orchid == null)
            {
                response.SetMessage(MessageId.I00000, "Orchid not found.");
                return response;
            }

            response.Response = new SelectOrchidEntity
            {
                OrchidId = orchid.OrchidId,
                OrchidName = orchid.OrchidName,
                OrchidDescription = orchid.OrchidDescription,
                OrchidUrl = orchid.OrchidUrl,
                Price = orchid.Price,
                IsNatural = orchid.IsNatural,
                CategoryId = orchid.CategoryId,
                CategoryName = orchid.Category?.CategoryName ?? "",
                CreatedAt = orchid.CreatedAt,
                CreatedBy = orchid.CreatedBy,
                IsActive = orchid.IsActive,
                UpdatedAt = orchid.UpdatedAt,
                UpdatedBy = orchid.UpdatedBy
            };

            response.Success = true;
            response.SetMessage(MessageId.I00000, "Orchid retrieved successfully.");
        }
        catch (Exception ex)
        {
            response.SetMessage(MessageId.E10000, $"Error retrieving orchid: {ex.Message}");
        }

        return response;
    }
}

// Handler cho multiple orchids
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

        try
        {
            // Lấy tất cả orchids từ OrchidCollection
            var query = await _orchidRepository.FindAllAsync(x => true);

            // Apply filters
            if (request.CategoryId.HasValue)
            {
                query = query.Where(x => x.CategoryId == request.CategoryId.Value).ToList();
            }

            if (request.IsNatural.HasValue)
            {
                query = query.Where(x => x.IsNatural == request.IsNatural.Value).ToList();
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive.Value).ToList();
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
                    CategoryName = orchid.Category?.CategoryName ?? "",
                    CreatedAt = orchid.CreatedAt,
                    CreatedBy = orchid.CreatedBy,
                    IsActive = orchid.IsActive,
                    UpdatedAt = orchid.UpdatedAt,
                    UpdatedBy = orchid.UpdatedBy
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            response.Success = true;
            response.SetMessage(MessageId.I00000, "Orchids retrieved successfully.");
        }
        catch (Exception ex)
        {
            response.SetMessage(MessageId.E10000, $"Error retrieving orchids: {ex.Message}");
        }

        return response;
    }
}
