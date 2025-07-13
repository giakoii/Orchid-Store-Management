using BackEnd.Utils.Const;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NLog;
using OrchidStore.Application.Features.Categories.Queries;
using OrchidStore.Application.Features;

namespace OrchidStore.API.Controllers.Categories;

/// <summary>
/// Controller for selecting multiple categories with filtering and pagination
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class SelectCategoriesController : AbstractApiAsyncControllerNotToken<SelectCategoriesQuery, SelectCategoriesResponse, SelectCategoriesData>
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="mediator"></param>
    public SelectCategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Incoming Get request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet]
    public override async Task<IActionResult> ProcessRequest([FromQuery] SelectCategoriesQuery request)
    {
        return await ProcessRequest(request, _logger, new SelectCategoriesResponse());
    }

    /// <summary>
    /// Main processing
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override async Task<SelectCategoriesResponse> Exec(SelectCategoriesQuery request)
    {
        return await _mediator.Send(request);
    }

    /// <summary>
    /// Error check
    /// </summary>
    /// <param name="request"></param>
    /// <param name="detailErrorList"></param>
    /// <returns></returns>
    protected internal override SelectCategoriesResponse ErrorCheck(SelectCategoriesQuery request, List<DetailError> detailErrorList)
    {
        var response = new SelectCategoriesResponse() { Success = false };
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
