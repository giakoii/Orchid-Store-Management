using BackEnd.Utils.Const;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NLog;
using OrchidStore.Application.Features;
using OrchidStore.Application.Features.Orchids.Queries;

namespace OrchidStore.API.Controllers.Orchids;

/// <summary>
/// Controller for selecting single orchid
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class SelectOrchidController : AbstractApiAsyncControllerNotToken<SelectOrchidQuery, SelectOrchidResponse, SelectOrchidEntity>
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public SelectOrchidController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Incoming Get request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet]
    public override async Task<IActionResult> ProcessRequest([FromQuery] SelectOrchidQuery request)
    {
        return await ProcessRequest(request, _logger, new SelectOrchidResponse());
    }

    /// <summary>
    /// Main processing
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override async Task<SelectOrchidResponse> Exec(SelectOrchidQuery request)
    {
        return await _mediator.Send(request);
    }

    /// <summary>
    /// Error check
    /// </summary>
    /// <param name="request"></param>
    /// <param name="detailErrorList"></param>
    /// <returns></returns>
    protected internal override SelectOrchidResponse ErrorCheck(SelectOrchidQuery request, List<DetailError> detailErrorList)
    {
        var response = new SelectOrchidResponse() { Success = false };
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
