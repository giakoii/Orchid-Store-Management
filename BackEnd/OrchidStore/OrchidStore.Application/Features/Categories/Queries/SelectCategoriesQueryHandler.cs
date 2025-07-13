using BackEnd.Utils.Const;
using OrchidStore.Application.CQRS;
using OrchidStore.Application.Repositories;
using OrchidStore.Domain.ReadModels;

namespace OrchidStore.Application.Features.Categories.Queries;

public class SelectCategoriesQueryHandler : IQueryHandler<SelectCategoriesQuery, SelectCategoriesResponse>
{
    private readonly IQueryRepository<CategoryCollection> _categoryRepository;

    public SelectCategoriesQueryHandler(IQueryRepository<CategoryCollection> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<SelectCategoriesResponse> Handle(SelectCategoriesQuery request, CancellationToken cancellationToken)
    {
        var response = new SelectCategoriesResponse { Success = false };
        
        // Select all active categories
        var query = await _categoryRepository.FindAllAsync(x => x.IsActive);
        
        // Apply filters
        if (request.ParentCategoryId.HasValue)
        {
            query = query.Where(x => x.ParentCategoryId == request.ParentCategoryId.Value).ToList();
        }

        if (!string.IsNullOrEmpty(request.SearchName))
        {
            query = query.Where(x => x.CategoryName.Contains(request.SearchName)).ToList();
        }

        // Get total count
        var totalCount = query.Count();

        // Apply pagination
        var categories = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        response.Response = new SelectCategoriesData
        {
            Categories = categories.Select(category => new SelectCategoryEntity
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.CategoryName,
                OrchidCount = category.Orchids?.Count(o => o.IsActive) ?? 0,
                ChildCategories = new List<SelectCategoryEntity>()
            }).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
        
        // True
        response.Success = true;
        response.SetMessage(MessageId.I00001);
        return response;
    }
}


public class SelectCategoryResponse : AbstractApiResponse<SelectCategoryEntity>
{
    public override SelectCategoryEntity Response { get; set; }
}

// SelectCategoriesQuery cho multiple categories (giữ nguyên)
public class SelectCategoriesQuery : AbstractApiRequest, IQuery<SelectCategoriesResponse>
{
    public int? ParentCategoryId { get; set; }
    public string? SearchName { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class SelectCategoriesResponse : AbstractApiResponse<SelectCategoriesData>
{
    public override SelectCategoriesData Response { get; set; }
}

public class SelectCategoriesData
{
    public List<SelectCategoryEntity> Categories { get; set; } = new List<SelectCategoryEntity>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class SelectCategoryEntity
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public int? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public int OrchidCount { get; set; }
    public List<SelectCategoryEntity> ChildCategories { get; set; }
}

