using BackEnd.Utils.Const;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using OrchidStore.Application.Features.Accounts.Queries;
using OrchidStore.Application.Features;
using OrchidStore.Application.Logics;

namespace OrchidStore.API.Controllers.Accounts;

/// <summary>
/// Controller for selecting account profile
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class SelectAccountProfileController : AbstractApiAsyncController<SelectAccountProfileQuery, SelectAccountProfileResponse, SelectAccountEntity>
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public SelectAccountProfileController(IMediator mediator, IIdentityService identityService)
    {
        _mediator = mediator;
        _identityService = identityService;
    }

    /// <summary>
    /// Incoming Get request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet]
    [Authorize(AuthenticationSchemes = OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public override async Task<IActionResult> ProcessRequest([FromQuery] SelectAccountProfileQuery request)
    {
        return await ProcessRequest(request, _logger, new SelectAccountProfileResponse());
    }

    /// <summary>
    /// Main processing
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override async Task<SelectAccountProfileResponse> Exec(SelectAccountProfileQuery request)
    {
        return await _mediator.Send(request);
    }

    /// <summary>
    /// Error check
    /// </summary>
    /// <param name="request"></param>
    /// <param name="detailErrorList"></param>
    /// <returns></returns>
    protected internal override SelectAccountProfileResponse ErrorCheck(SelectAccountProfileQuery request, List<DetailError> detailErrorList)
    {
        var response = new SelectAccountProfileResponse() { Success = false };
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