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
    [Required(ErrorMessage = "Orchid ID is required.")]
    public int OrchidId { get; set; }
}

/// <summary>
/// Command handler for deleting an orchid.
/// </summary>
public class OrchidDeleteCommandHandler : ICommandHandler<CategoryDeleteCommand, CommandResponse>
{
    private readonly ICommandRepository<Orchid> _orchidRepository;
    private readonly IIdentityService _identityService;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="orchidRepository"></param>
    /// <param name="identityService"></param>
    public OrchidDeleteCommandHandler(ICommandRepository<Orchid> orchidRepository, IIdentityService identityService)
    {
        _orchidRepository = orchidRepository;
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
        
        // Validate OrchidId
        var orchidExists = await _orchidRepository.Find(x => x.OrchidId == request.OrchidId).FirstOrDefaultAsync();
        if (orchidExists == null)
        {
            response.SetMessage(MessageId.I00000, "Orchid not found.");
            return response;
        }
        
        // Begin transaction
        await _orchidRepository.ExecuteInTransactionAsync(async () =>
        {
            // Mark the orchid as deleted
            _orchidRepository.Update(orchidExists);
            await _orchidRepository.SaveChangesAsync(userEmail, true);
            
            // Soft delete the orchid in collection
            _orchidRepository.Store(OrchidCollection.FromWriteModel(orchidExists, true), userEmail, true, true);
            await _orchidRepository.SessionSavechanges();
            
            // True
            response.Success = true;
            response.SetMessage(MessageId.I00001);
            return true;
        });
        return response;
    }
}