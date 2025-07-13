using System.ComponentModel.DataAnnotations;
using BackEnd.Utils.Const;
using Microsoft.EntityFrameworkCore;
using OrchidStore.Application.CQRS;
using OrchidStore.Application.Logics;
using OrchidStore.Application.Repositories;
using OrchidStore.Domain.ReadModels;
using OrchidStore.Domain.WriteModels;

namespace OrchidStore.Application.Features.Categories.Commands;

public class CategoryDeleteCommand : AbstractApiRequest, ICommand<CommandResponse>
{
    [Required(ErrorMessage = "Category ID is required.")]
    public int CategoryId { get; set; }
}

/// <summary>
/// Command handler for deleting an orchid.
/// </summary>
public class CategoryDeleteCommandHandler : ICommandHandler<CategoryDeleteCommand, CommandResponse>
{
    private readonly ICommandRepository<Category> _categoryRepository;
    private readonly IIdentityService _identityService;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="categoryRepository"></param>
    /// <param name="identityService"></param>
    public CategoryDeleteCommandHandler(ICommandRepository<Category> categoryRepository, IIdentityService identityService)
    {
        _categoryRepository = categoryRepository;
        _identityService = identityService;
    }

    /// <summary>
    /// Handles the orchid deletion command.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<CommandResponse> Handle(CategoryDeleteCommand request, CancellationToken cancellationToken)
    {
        var response = new CommandResponse { Success = false };
        var userEmail = _identityService.GetCurrentUser().Email;
        
        // Validate CategoryId
        var categoryExist = await _categoryRepository.Find(x => x.CategoryId == request.CategoryId).FirstOrDefaultAsync();
        if (categoryExist == null)
        {
            response.SetMessage(MessageId.I00000, "Category not found.");
            return response;
        }
        
        // Begin transaction
        await _categoryRepository.ExecuteInTransactionAsync(async () =>
        {
            // Mark the orchid as deleted
            _categoryRepository.Update(categoryExist);
            await _categoryRepository.SaveChangesAsync(userEmail, true);
            
            // Soft delete the orchid in collection
            _categoryRepository.Store(CategoryCollection.FromWriteModel(categoryExist, true), userEmail, true, true);
            await _categoryRepository.SessionSavechanges();
            
            // True
            response.Success = true;
            response.SetMessage(MessageId.I00001);
            return true;
        });
        return response;
    }
}