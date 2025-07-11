using BackEnd.Utils.Const;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NLog;
using OrchidStore.Application.Features;
using OrchidStore.Application.Features.Accounts.Commands;
using OrchidStore.API.SystemClient;

namespace OrchidStore.API.Controllers.Accounts;

/// <summary>
/// Controller for updating account
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class UpdateAccountController : AbstractApiAsyncController<AccountUpdateCommand, CommandResponse, string>
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public UpdateAccountController(IMediator mediator, IIdentityApiClient identityApiClient)
    {
        _mediator = mediator;
        _identityApiClient = identityApiClient;
    }

    /// <summary>
    /// Incoming Put request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut]
    public override async Task<IActionResult> ProcessRequest(AccountUpdateCommand request)
    {
        return await ProcessRequest(request, _logger, new CommandResponse());
    }

    /// <summary>
    /// Main processing
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override async Task<CommandResponse> Exec(AccountUpdateCommand request)
    {
        return await _mediator.Send(request);
    }

    /// <summary>
    /// Error check
    /// </summary>
    /// <param name="request"></param>
    /// <param name="detailErrorList"></param>
    /// <returns></returns>
    protected internal override CommandResponse ErrorCheck(AccountUpdateCommand request, List<DetailError> detailErrorList)
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
