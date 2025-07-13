using BackEnd.Utils.Const;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using OpenIddict.Validation.AspNetCore;
using OrchidStore.Application.Features;
using OrchidStore.Application.Features.Categories.Commands;
using OrchidStore.Application.Logics;

namespace OrchidStore.API.Controllers.Categories;

/// <summary>
/// DeleteCategoryController - Deletes an orchid.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class DeleteCategoryController : AbstractApiAsyncController<CategoryDeleteCommand, CommandResponse, string>
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="identityService"></param>
    public DeleteCategoryController(IMediator mediator, IIdentityService identityService)
    {
        _mediator = mediator;
        _identityService = identityService;
    }
    
    /// <summary>
    /// Incoming Patch
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPatch]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public override async Task<IActionResult> ProcessRequest(CategoryDeleteCommand request)
    {
        return await ProcessRequest(request, _logger, new CommandResponse());
    }

    /// <summary>
    /// Main processing
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override async Task<CommandResponse> Exec(CategoryDeleteCommand request)
    {
        return await _mediator.Send(request);
    }

    /// <summary>
    /// Error check
    /// </summary>
    /// <param name="request"></param>
    /// <param name="detailErrorList"></param>
    /// <returns></returns>
    protected internal override CommandResponse ErrorCheck(CategoryDeleteCommand request, List<DetailError> detailErrorList)
    {
        var response = new CommandResponse() { Success = false };
        if (detailErrorList.Count > 0)
        {
            // Error
            response.SetMessage(MessageId.E10000);
            response.DetailErrorList = detailErrorList;
            return response;
        }
        // True
        response.Success = true;
        return response;
    }
}