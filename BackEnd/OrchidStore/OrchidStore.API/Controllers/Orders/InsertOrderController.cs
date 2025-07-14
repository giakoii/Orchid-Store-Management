using BackEnd.Utils.Const;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using OpenIddict.Validation.AspNetCore;
using OrchidStore.Application.Features;
using OrchidStore.Application.Features.Orders.Commands;
using OrchidStore.Application.Logics;

namespace OrchidStore.API.Controllers.Orders;

/// <summary>
/// InsertOrderController - Inserts a new order.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class InsertOrderController : AbstractApiAsyncController<OrderInsertCommand, OrderInsertCommandResponse, MomoResponse>
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="identityService"></param>
    public InsertOrderController(IMediator mediator, IIdentityService identityService)
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
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public override async Task<IActionResult> ProcessRequest(OrderInsertCommand request)
    {
        return await ProcessRequest(request, _logger, new OrderInsertCommandResponse());
    }

    /// <summary>
    /// Main processing
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override async Task<OrderInsertCommandResponse> Exec(OrderInsertCommand request)
    {
        return await _mediator.Send(request);
    }

    /// <summary>
    /// Error check
    /// </summary>
    /// <param name="request"></param>
    /// <param name="detailErrorList"></param>
    /// <returns></returns>
    protected internal override OrderInsertCommandResponse ErrorCheck(OrderInsertCommand request, List<DetailError> detailErrorList)
    {
        var response = new OrderInsertCommandResponse() { Success = false };
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

/// <summary>
/// PaymentOrderCallbackController - Handles payment callbacks for orders.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class PaymentOrderCallbackController : AbstractApiAsyncController<OrderPaymentCallbackCommand, OrderInsertCommandResponse, MomoResponse>
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="identityService"></param>
    public PaymentOrderCallbackController(IMediator mediator, IIdentityService identityService)
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
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public override async Task<IActionResult> ProcessRequest(OrderPaymentCallbackCommand request)
    {
        return await ProcessRequest(request, _logger, new OrderInsertCommandResponse());
    }

    /// <summary>
    /// Main processing
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override async Task<OrderInsertCommandResponse> Exec(OrderPaymentCallbackCommand request)
    {
        return await _mediator.Send(request);
    }

    /// <summary>
    /// Error check
    /// </summary>
    /// <param name="request"></param>
    /// <param name="detailErrorList"></param>
    /// <returns></returns>
    protected internal override OrderInsertCommandResponse ErrorCheck(OrderPaymentCallbackCommand request, List<DetailError> detailErrorList)
    {
        var response = new OrderInsertCommandResponse() { Success = false };
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
