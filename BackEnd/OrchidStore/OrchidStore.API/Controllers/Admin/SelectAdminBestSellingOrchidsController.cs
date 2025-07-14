using BackEnd.Utils.Const;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using OpenIddict.Validation.AspNetCore;
using OrchidStore.Application.Features;
using OrchidStore.Application.Features.Admin.Queries;
using OrchidStore.Application.Logics;
using OrchidStore.Application.Utils.Const;

namespace OrchidStore.API.Controllers.Admin;

/// <summary>
/// Controller for selecting admin best selling orchids
/// </summary>
[ApiController]
[Route("api/v1/admin/[controller]")]
public class SelectAdminBestSellingOrchidsController : AbstractApiAsyncController<SelectAdminBestSellingOrchidsQuery, SelectAdminBestSellingOrchidsQueryResponse, SelectAdminBestSellingOrchidsEntity>
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public SelectAdminBestSellingOrchidsController(IMediator mediator, IIdentityService identityService)
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
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Roles = ConstRole.Admin)]
    public override async Task<IActionResult> ProcessRequest([FromQuery] SelectAdminBestSellingOrchidsQuery request)
    {
        return await ProcessRequest(request, _logger, new SelectAdminBestSellingOrchidsQueryResponse());
    }

    /// <summary>
    /// Main processing
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override async Task<SelectAdminBestSellingOrchidsQueryResponse> Exec(SelectAdminBestSellingOrchidsQuery request)
    {
        return await _mediator.Send(request);
    }

    /// <summary>
    /// Error check
    /// </summary>
    /// <param name="request"></param>
    /// <param name="detailErrorList"></param>
    /// <returns></returns>
    protected internal override SelectAdminBestSellingOrchidsQueryResponse ErrorCheck(SelectAdminBestSellingOrchidsQuery request, List<DetailError> detailErrorList)
    {
        var response = new SelectAdminBestSellingOrchidsQueryResponse() { Success = false };
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
