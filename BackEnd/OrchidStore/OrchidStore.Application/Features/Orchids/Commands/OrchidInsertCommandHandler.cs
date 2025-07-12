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

public class OrchidInsertCommand : AbstractApiRequest, ICommand<CommandResponse>
{
    
    [Required(ErrorMessage = "Orchid name is required.")]
    public string OrchidName { get; set; } = null!;

    [Required(ErrorMessage = "Orchid description is required.")]
    public required string OrchidDescription { get; set; }

    [Required(ErrorMessage = "Orchid image is required.")]
    public required IFormFile OrchidImage { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Category ID is required.")]
    public int CategoryId { get; set; }

    public bool IsNatural { get; set; } = false;
}

/// <summary>
/// Command handler for inserting a new orchid.
/// </summary>
public class OrchidInsertCommandHandler : ICommandHandler<OrchidInsertCommand, CommandResponse>
{
    private readonly ICommandRepository<Orchid> _orchidRepository;
    private readonly ICommandRepository<Category> _categoryRepository;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IIdentityService _identityService;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="orchidRepository"></param>
    /// <param name="categoryRepository"></param>
    /// <param name="identityService"></param>
    /// <param name="cloudinaryService"></param>
    public OrchidInsertCommandHandler(ICommandRepository<Orchid> orchidRepository, ICommandRepository<Category> categoryRepository, IIdentityService identityService, ICloudinaryService cloudinaryService)
    {
        _orchidRepository = orchidRepository;
        _categoryRepository = categoryRepository;
        _identityService = identityService;
        _cloudinaryService = cloudinaryService;
    }

    /// <summary>
    /// Handles the orchid insert command.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<CommandResponse> Handle(OrchidInsertCommand request, CancellationToken cancellationToken)
    {
        var response = new CommandResponse { Success = false };

        try
        {
            // Get current user email
            var currentUserEmail = _identityService.GetCurrentUser().Email;

            // Validate category exists
            var categoryExists = await _categoryRepository.Find(x => x.CategoryId == request.CategoryId && x.IsActive).FirstOrDefaultAsync(cancellationToken);
            if (categoryExists == null)
            {
                response.SetMessage(MessageId.I00000, "Category not found or inactive.");
                return response;
            }

            // Check if orchid name already exists
            var orchidExists = await _orchidRepository.Find(x => x.OrchidName == request.OrchidName).FirstOrDefaultAsync(cancellationToken);
            if (orchidExists != null)
            {
                response.SetMessage(MessageId.I00000, "Orchid name already exists.");
                return response;
            }

            // Begin transaction
            await _orchidRepository.ExecuteInTransactionAsync(async () =>
            {
                // Create new orchid
                var newOrchid = new Orchid
                {
                    OrchidName = request.OrchidName,
                    OrchidDescription = request.OrchidDescription,
                    Price = request.Price,
                    CategoryId = request.CategoryId,
                    IsNatural = request.IsNatural,
                };
                
                // Upload image to Cloudinary
                newOrchid.OrchidUrl = await _cloudinaryService.UploadImageAsync(request.OrchidImage);

                // Context save changes
                await _orchidRepository.AddAsync(newOrchid);
                await _orchidRepository.SaveChangesAsync(currentUserEmail);

                // Session save changes
                _orchidRepository.Store(OrchidCollection.FromWriteModel(newOrchid, true), currentUserEmail);
                await _orchidRepository.SessionSavechanges();
                
                // True
                response.Success = true;
                response.SetMessage(MessageId.I00001);
                return true;
            });
        }
        catch (Exception ex)
        {
            response.SetMessage(MessageId.E10000, $"Error creating orchid: {ex.Message}");
        }

        return response;
    }
}