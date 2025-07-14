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
/// Controller for selecting admin orders statistics
/// </summary>
[ApiController]
[Route("api/v1/admin/[controller]")]
public class SelectAdminOrdersStatisticsController : AbstractApiAsyncController<AdminOrdersStatisticsSelectQuery, AdminOrdersStatisticsSelectQueryResponse, AdminOrdersStatisticsEntity>
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public SelectAdminOrdersStatisticsController(IMediator mediator, IIdentityService identityService)
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
    [Authorize(Roles = ConstRole.Admin, AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]   
    public override async Task<IActionResult> ProcessRequest([FromQuery] AdminOrdersStatisticsSelectQuery request)
    {
        return await ProcessRequest(request, _logger, new AdminOrdersStatisticsSelectQueryResponse());
    }

    /// <summary>
    /// Main processing
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override async Task<AdminOrdersStatisticsSelectQueryResponse> Exec(AdminOrdersStatisticsSelectQuery request)
    {
        return await _mediator.Send(request);
    }

    /// <summary>
    /// Error check
    /// </summary>
    /// <param name="request"></param>
    /// <param name="detailErrorList"></param>
    /// <returns></returns>
    protected internal override AdminOrdersStatisticsSelectQueryResponse ErrorCheck(AdminOrdersStatisticsSelectQuery request, List<DetailError> detailErrorList)
    {
        var response = new AdminOrdersStatisticsSelectQueryResponse() { Success = false };
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
