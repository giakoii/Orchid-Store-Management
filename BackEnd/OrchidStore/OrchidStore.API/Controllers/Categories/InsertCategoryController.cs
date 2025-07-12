using BackEnd.Utils.Const;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using OrchidStore.Application.Features;
using OrchidStore.Application.Features.Categories.Commands;
using OrchidStore.Application.Logics;

namespace OrchidStore.API.Controllers.Categories;

/// <summary>
/// Controller for inserting category
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class InsertCategoryController : AbstractApiAsyncController<CategoryInsertCommand, CommandResponse, string>
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public InsertCategoryController(IMediator mediator, IIdentityService identityService)
    {
        _mediator = mediator;
        _identityService = identityService;
    }

    /// <summary>
    /// Incoming Post request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(AuthenticationSchemes = OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public override async Task<IActionResult> ProcessRequest(CategoryInsertCommand request)
    {
        return await ProcessRequest(request, _logger, new CommandResponse());
    }

    /// <summary>
    /// Main processing
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override async Task<CommandResponse> Exec(CategoryInsertCommand request)
    {
        return await _mediator.Send(request);
    }

    /// <summary>
    /// Error check
    /// </summary>
    /// <param name="request"></param>
    /// <param name="detailErrorList"></param>
    /// <returns></returns>
    protected internal override CommandResponse ErrorCheck(CategoryInsertCommand request, List<DetailError> detailErrorList)
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
