using BackEnd.Utils.Const;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using OrchidStore.Application.Features;
using OrchidStore.Application.Features.Orchids.Commands;
using OrchidStore.Application.Logics;
using OrchidStore.Application.Utils.Const;

namespace OrchidStore.API.Controllers.Orchids;

/// <summary>
/// Controller for inserting orchid
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class InsertOrchidController : AbstractApiAsyncController<OrchidInsertCommand, CommandResponse, string>
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public InsertOrchidController(IMediator mediator, IIdentityService identityService)
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
    [Authorize(Roles = ConstRole.Admin, AuthenticationSchemes = OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public override async Task<IActionResult> ProcessRequest(OrchidInsertCommand request)
    {
        return await ProcessRequest(request, _logger, new CommandResponse());
    }

    /// <summary>
    /// Main processing
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override async Task<CommandResponse> Exec(OrchidInsertCommand request)
    {
        return await _mediator.Send(request);
    }

    /// <summary>
    /// Error check
    /// </summary>
    /// <param name="request"></param>
    /// <param name="detailErrorList"></param>
    /// <returns></returns>
    protected internal override CommandResponse ErrorCheck(OrchidInsertCommand request, List<DetailError> detailErrorList)
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
