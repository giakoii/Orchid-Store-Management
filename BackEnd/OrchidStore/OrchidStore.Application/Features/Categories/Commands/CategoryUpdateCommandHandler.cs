using System.ComponentModel.DataAnnotations;
using BackEnd.Utils.Const;
using Microsoft.EntityFrameworkCore;
using OrchidStore.Application.CQRS;
using OrchidStore.Application.Logics;
using OrchidStore.Application.Repositories;
using OrchidStore.Domain.ReadModels;
using OrchidStore.Domain.WriteModels;

namespace OrchidStore.Application.Features.Categories.Commands;

public class CategoryUpdateCommand : AbstractApiRequest, ICommand<CommandResponse>
{
    [Required(ErrorMessage = "Category ID is required.")]
    public int CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public int? ParentCategoryId { get; set; }
}

public class CategoryUpdateCommandHandler : ICommandHandler<CategoryUpdateCommand, CommandResponse>
{
    private readonly ICommandRepository<Category> _categoryRepository;
    private readonly IIdentityService _identityService;

    public CategoryUpdateCommandHandler(ICommandRepository<Category> categoryRepository, IIdentityService identityService)
    {
        _categoryRepository = categoryRepository;
        _identityService = identityService;
    }

    /// <summary>
    /// Handles the category update command.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<CommandResponse> Handle(CategoryUpdateCommand request, CancellationToken cancellationToken)
    {
        var response = new CommandResponse { Success = false };
        var currentUser = _identityService.GetCurrentUser();
        
        // Find existing category
        var existingCategory = await _categoryRepository.Find(x => x.CategoryId == request.CategoryId).FirstOrDefaultAsync(cancellationToken);
        if (existingCategory == null)
        {
            response.SetMessage(MessageId.I00000, "Category not found.");
            return response;
        }

        // Validate category name if provided and different from current
        if (!string.IsNullOrEmpty(request.CategoryName) && request.CategoryName != existingCategory.CategoryName)
        {
            var nameExists = await _categoryRepository
                .Find(x => x.CategoryName == request.CategoryName && x.CategoryId != request.CategoryId)
                .FirstOrDefaultAsync(cancellationToken);
            if (nameExists != null)
            {
                response.SetMessage(MessageId.I00000, "Category name already exists.");
                return response;
            }

            existingCategory.CategoryName = request.CategoryName;
        }

        // Validate parent category if provided
        if (request.ParentCategoryId.HasValue)
        {
            // Prevent self-reference
            if (request.ParentCategoryId.Value == request.CategoryId)
            {
                response.SetMessage(MessageId.I00000, "Category cannot be its own parent.");
                return response;
            }

            var parentExists = await _categoryRepository
                .Find(x => x.CategoryId == request.ParentCategoryId.Value && x.IsActive)
                .FirstOrDefaultAsync(cancellationToken);

            if (parentExists == null)
            {
                response.SetMessage(MessageId.I00000, "Parent category not found or inactive.");
                return response;
            }

            existingCategory.ParentCategoryId = request.ParentCategoryId.Value;
        }

        // Context Save changes
        _categoryRepository.Update(existingCategory);
        await _categoryRepository.SaveChangesAsync(currentUser.Email);
        
        // Session save changes
        _categoryRepository.Store(CategoryCollection.FromWriteModel(existingCategory), currentUser.Email, true);
        await _categoryRepository.SessionSavechanges();

        response.Success = true;
        response.SetMessage(MessageId.I00001);
        return response;
    }
}