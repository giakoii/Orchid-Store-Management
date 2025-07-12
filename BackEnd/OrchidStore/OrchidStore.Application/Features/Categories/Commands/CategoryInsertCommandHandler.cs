using System.ComponentModel.DataAnnotations;
using BackEnd.Utils.Const;
using Microsoft.EntityFrameworkCore;
using OrchidStore.Application.CQRS;
using OrchidStore.Application.Logics;
using OrchidStore.Application.Repositories;
using OrchidStore.Domain.ReadModels;
using OrchidStore.Domain.WriteModels;

namespace OrchidStore.Application.Features.Categories.Commands;

public class CategoryInsertCommand : AbstractApiRequest, ICommand<CommandResponse>
{
    [Required(ErrorMessage = "Category name is required.")]
    public string CategoryName { get; set; } = null!;

    public int? ParentCategoryId { get; set; }
}

public class CategoryInsertCommandHandler : ICommandHandler<CategoryInsertCommand, CommandResponse>
{
    private readonly ICommandRepository<Category> _categoryRepository;
    private readonly IIdentityService _identityService;


    public CategoryInsertCommandHandler(ICommandRepository<Category> categoryRepository, IIdentityService identityService)
    {
        _categoryRepository = categoryRepository;
        _identityService = identityService;
    }

    /// <summary>
    /// Handles the category insert command.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<CommandResponse> Handle(CategoryInsertCommand request, CancellationToken cancellationToken)
    {
        var response = new CommandResponse { Success = false };
        var currentUser = _identityService.GetCurrentUser();

        // Check if category name already exists
        var categoryExists = await _categoryRepository.Find(x => x.CategoryName == request.CategoryName)
            .FirstOrDefaultAsync(cancellationToken);
        if (categoryExists != null)
        {
            response.SetMessage(MessageId.I00000, "Category name already exists.");
            return response;
        }

        // Validate parent category if provided
        if (request.ParentCategoryId.HasValue)
        {
            var parentExists = await _categoryRepository
                .Find(x => x.CategoryId == request.ParentCategoryId.Value && x.IsActive)
                .FirstOrDefaultAsync(cancellationToken);
            if (parentExists == null)
            {
                response.SetMessage(MessageId.I00000, "Parent category not found or inactive.");
                return response;
            }
        }

        // Begin transaction
        await _categoryRepository.ExecuteInTransactionAsync(async () =>
        {
            // Insert new category
            var newCategory = new Category
            {
                CategoryName = request.CategoryName,
                ParentCategoryId = request.ParentCategoryId,
            };

            // Context save changes
            await _categoryRepository.AddAsync(newCategory);
            await _categoryRepository.SaveChangesAsync(currentUser.Email);
        
            // Session save changes
            _categoryRepository.Store(CategoryCollection.FromWriteModel(newCategory, true), currentUser.Email);
            await _categoryRepository.SessionSavechanges();

            response.Success = true;
            response.SetMessage(MessageId.I00001);
            return true;
        });
        return response;
    }
}