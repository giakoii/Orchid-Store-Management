using System.ComponentModel.DataAnnotations;
using BackEnd.Utils.Const;
using OrchidStore.Application.CQRS;
using OrchidStore.Application.Repositories;
using OrchidStore.Domain.ReadModels;

namespace OrchidStore.Application.Features.Categories.Queries;

public class SelectCategoryQuery : AbstractApiRequest, IQuery<SelectCategoryResponse>
{
    [Required(ErrorMessage = "Category ID is required.")]
    public int CategoryId { get; set; }
}

public class SelectCategoryQueryHandler : IQueryHandler<SelectCategoryQuery, SelectCategoryResponse>
{
    private readonly IQueryRepository<CategoryCollection> _categoryRepository;

    public SelectCategoryQueryHandler(IQueryRepository<CategoryCollection> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<SelectCategoryResponse> Handle(SelectCategoryQuery request, CancellationToken cancellationToken)
    {
        var response = new SelectCategoryResponse { Success = false };

        var category = await _categoryRepository.FindOneAsync(x => x.CategoryId == request.CategoryId);

        if (category == null)
        {
            response.SetMessage(MessageId.I00000, "Category not found.");
            return response;
        }

        response.Response = new SelectCategoryEntity
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = category.ParentCategory?.CategoryName,
            OrchidCount = category.Orchids?.Count(o => o.IsActive) ?? 0,
            ChildCategories = category.SubCategories?.Where(c => c.IsActive).Select(child => new SelectCategoryEntity
            {
                CategoryId = child.CategoryId,
                CategoryName = child.CategoryName,
                ParentCategoryId = child.ParentCategoryId,
                ParentCategoryName = child.ParentCategory?.CategoryName,
                OrchidCount = child.Orchids?.Count(o => o.IsActive) ?? 0
            }).ToList() ?? new List<SelectCategoryEntity>()
        };

        // True
        response.Success = true;
        response.SetMessage(MessageId.I00001);
        return response;
    }
}