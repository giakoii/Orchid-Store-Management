using System.ComponentModel.DataAnnotations;
using BackEnd.Utils.Const;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OrchidStore.Application.CQRS;
using OrchidStore.Application.Logics;
using OrchidStore.Application.Repositories;
using OrchidStore.Domain.ReadModels;
using OrchidStore.Domain.WriteModels;

namespace OrchidStore.Application.Features.Orchids.Commands;

public class OrchidUpdateCommand : AbstractApiRequest, ICommand<CommandResponse>
{
    [Required(ErrorMessage = "Orchid ID is required.")]
    public int OrchidId { get; set; }

    public string? OrchidName { get; set; }

    public string? OrchidDescription { get; set; }

    public IFormFile? OrchidImage { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
    public decimal? Price { get; set; }

    public int? CategoryId { get; set; }

    public bool? IsNatural { get; set; }
}

/// <summary>
/// Command handler for updating an existing orchid.
/// </summary>
public class OrchidUpdateCommandHandler : ICommandHandler<OrchidUpdateCommand, CommandResponse>
{
    private readonly ICommandRepository<Orchid> _orchidRepository;
    private readonly ICommandRepository<Category> _categoryRepository;
    private readonly IIdentityService _identityService;
    private readonly ICloudinaryService _cloudinaryService;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="orchidRepository"></param>
    /// <param name="categoryRepository"></param>
    /// <param name="cloudinaryService"></param>
    /// <param name="identityService"></param>
    public OrchidUpdateCommandHandler(ICommandRepository<Orchid> orchidRepository,
        ICommandRepository<Category> categoryRepository, ICloudinaryService cloudinaryService,
        IIdentityService identityService)
    {
        _orchidRepository = orchidRepository;
        _categoryRepository = categoryRepository;
        _cloudinaryService = cloudinaryService;
        _identityService = identityService;
    }

    /// <summary>
    /// Handles the orchid update command.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<CommandResponse> Handle(OrchidUpdateCommand request, CancellationToken cancellationToken)
    {
        var response = new CommandResponse { Success = false };

        // Get current user's email
        var currentEmail = _identityService.GetCurrentUser()?.Email;

        // Find existing orchid
        var existingOrchid = await _orchidRepository.Find(x => x.OrchidId == request.OrchidId)
            .FirstOrDefaultAsync(cancellationToken);
        if (existingOrchid == null)
        {
            response.SetMessage(MessageId.I00000, "Orchid not found.");
            return response;
        }

        // Begin transaction
        await _orchidRepository.ExecuteInTransactionAsync(async () =>
        {
            // Validate orchid name if provided and different from current
            if (!string.IsNullOrEmpty(request.OrchidName) && request.OrchidName != existingOrchid.OrchidName)
            {
                var nameExists = await _orchidRepository
                    .Find(x => x.OrchidName == request.OrchidName && x.OrchidId != request.OrchidId)
                    .FirstOrDefaultAsync(cancellationToken);
                if (nameExists != null)
                {
                    response.SetMessage(MessageId.I00000, "Orchid name already exists.");
                    return false;
                }

                existingOrchid.OrchidName = request.OrchidName;
            }

            // Validate category if provided
            if (request.CategoryId.HasValue)
            {
                var categoryExists = await _categoryRepository
                    .Find(x => x.CategoryId == request.CategoryId.Value && x.IsActive)
                    .FirstOrDefaultAsync(cancellationToken);
                if (categoryExists == null)
                {
                    response.SetMessage(MessageId.I00000, "Category not found or inactive.");
                    return false;
                }

                existingOrchid.CategoryId = request.CategoryId.Value;
            }

            // Update fields if provided
            if (request.OrchidDescription != null)
            {
                existingOrchid.OrchidDescription = request.OrchidDescription;
            }

            if (request.OrchidImage != null)
            {
                // Upload new image and update URL
                var uploadResult = await _cloudinaryService.UploadImageAsync(request.OrchidImage);
                existingOrchid.OrchidUrl = uploadResult;
            }

            if (request.Price.HasValue)
            {
                existingOrchid.Price = request.Price.Value;
            }

            if (request.IsNatural.HasValue)
            {
                existingOrchid.IsNatural = request.IsNatural.Value;
            }

            // Save changes
            _orchidRepository.Update(existingOrchid);
            await _orchidRepository.SaveChangesAsync(currentEmail!);

            // Store updated orchid in session
            _orchidRepository.Store(OrchidCollection.FromWriteModel(existingOrchid, true), currentEmail!, true);
            await _orchidRepository.SessionSavechanges();

            // True
            response.Success = true;
            response.SetMessage(MessageId.I00001);
            return true;
        });
        return response;
    }
}