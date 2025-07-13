using System.ComponentModel.DataAnnotations;
using BackEnd.Utils.Const;
using OrchidStore.Application.CQRS;
using OrchidStore.Application.Repositories;
using OrchidStore.Domain.ReadModels;

namespace OrchidStore.Application.Features.Orchids.Queries;

public class SelectOrchidQuery : AbstractApiRequest, IQuery<SelectOrchidResponse>
{
    [Required(ErrorMessage = "Orchid ID is required.")]
    public int OrchidId { get; set; }
}

/// <summary>
/// Query handler for selecting an orchid by ID.
/// </summary>
public class SelectOrchidQueryHandler : IQueryHandler<SelectOrchidQuery, SelectOrchidResponse>
{
    private readonly IQueryRepository<OrchidCollection> _orchidRepository;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="orchidRepository"></param>
    public SelectOrchidQueryHandler(IQueryRepository<OrchidCollection> orchidRepository)
    {
        _orchidRepository = orchidRepository;
    }

    /// <summary>
    /// Handles the select orchid query by ID.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<SelectOrchidResponse> Handle(SelectOrchidQuery request, CancellationToken cancellationToken)
    {
        var response = new SelectOrchidResponse { Success = false };

        var orchid = await _orchidRepository.FindOneAsync(x => x.OrchidId == request.OrchidId && x.IsActive);
        if (orchid == null)
        {
            response.SetMessage(MessageId.I00000, "Orchid not found.");
            return response;
        }
        
        // Set response data
        response.Response = new SelectOrchidEntity
        {
            OrchidId = orchid.OrchidId,
            OrchidName = orchid.OrchidName,
            OrchidDescription = orchid.OrchidDescription,
            OrchidUrl = orchid.OrchidUrl,
            Price = orchid.Price,
            IsNatural = orchid.IsNatural,
            CategoryId = orchid.CategoryId,
            CategoryName = orchid.Category.CategoryName,
            CreatedAt = orchid.CreatedAt,
            CreatedBy = orchid.CreatedBy,
        };

        // True
        response.Success = true;
        response.SetMessage(MessageId.I00001);
        return response;
    }
}

public class SelectOrchidResponse : AbstractApiResponse<SelectOrchidEntity>
{
    public override SelectOrchidEntity Response { get; set; }
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
}