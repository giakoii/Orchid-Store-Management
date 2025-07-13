using BackEnd.Utils.Const;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NLog;
using OrchidStore.Application.Features.Categories.Queries;
using OrchidStore.Application.Features;
using OrchidStore.Application.Logics;

namespace OrchidStore.API.Controllers.Categories;

/// <summary>
/// Controller for selecting single category
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class SelectCategoryController : AbstractApiAsyncControllerNotToken<SelectCategoryQuery, SelectCategoryResponse, SelectCategoryEntity>
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="mediator"></param>
    public SelectCategoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Incoming Get request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet]
    public override async Task<IActionResult> ProcessRequest([FromQuery] SelectCategoryQuery request)
    {
        return await ProcessRequest(request, _logger, new SelectCategoryResponse());
    }

    /// <summary>
    /// Main processing
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override async Task<SelectCategoryResponse> Exec(SelectCategoryQuery request)
    {
        return await _mediator.Send(request);
    }

    /// <summary>
    /// Error check
    /// </summary>
    /// <param name="request"></param>
    /// <param name="detailErrorList"></param>
    /// <returns></returns>
    protected internal override SelectCategoryResponse ErrorCheck(SelectCategoryQuery request, List<DetailError> detailErrorList)
    {
        var response = new SelectCategoryResponse() { Success = false };
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
