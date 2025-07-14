using BackEnd.Utils.Const;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using OpenIddict.Validation.AspNetCore;
using OrchidStore.Application.Features;
using OrchidStore.Application.Features.Orders.Queries;
using OrchidStore.Application.Logics;

namespace OrchidStore.API.Controllers.Orders;

/// <summary>
/// SelectOrderController - Select order by accountId.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class SelectOrdersController : AbstractApiAsyncController<OrdersSelectQuery, OrdersSelectQueryResponse, List<OrdersSelectEntity>>
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="identityService"></param>
    public SelectOrdersController(IMediator mediator, IIdentityService identityService)
    {
        _mediator = mediator;
        _identityService = identityService;
    }

    /// <summary>
    /// Incoming Post request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public override async Task<IActionResult> ProcessRequest([FromQuery]OrdersSelectQuery request)
    {
        return await ProcessRequest(request, _logger, new OrdersSelectQueryResponse());
    }

    /// <summary>
    /// Main processing
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override async Task<OrdersSelectQueryResponse> Exec(OrdersSelectQuery request)
    {
        return await _mediator.Send(request);
    }

    /// <summary>
    /// Error check
    /// </summary>
    /// <param name="request"></param>
    /// <param name="detailErrorList"></param>
    /// <returns></returns>
    protected internal override OrdersSelectQueryResponse ErrorCheck(OrdersSelectQuery request, List<DetailError> detailErrorList)
    {
        var response = new OrdersSelectQueryResponse() { Success = false };
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