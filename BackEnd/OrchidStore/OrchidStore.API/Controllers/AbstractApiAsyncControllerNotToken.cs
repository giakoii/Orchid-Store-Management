using MediatR;
using Microsoft.AspNetCore.Mvc;
using NLog;
using OrchidStore.Application.Features;
using OrchidStore.Application.LogConfig;

namespace OrchidStore.API.Controllers;

public abstract class AbstractApiAsyncControllerNotToken
    <T, U, V> : ControllerBase
    where T : class
    where U : AbstractApiResponse<V>
{
         /// <summary>
        /// API entry point
        /// </summary>
        public abstract Task<IActionResult> ProcessRequest(T request);

        /// <summary>
        /// Main processing (business logic)
        /// </summary>
        protected abstract Task<U> Exec(T request);

        /// <summary>
        /// Error check method to be implemented by subclasses
        /// </summary>
        protected internal abstract U ErrorCheck(T request, List<DetailError> detailErrorList);

        /// <summary>
        /// Mediator
        /// </summary>
        protected IMediator _mediator;

        /// <summary>
        /// Template Method
        /// </summary>
        protected async Task<IActionResult> ProcessRequest(T request, Logger logger, U returnValue)
        {
            var loggingUtil = new LoggingUtil(logger, "Anonymous User");
            loggingUtil.StartLog(request);
            
            try
            {
                // Model validation errors
                var detailErrorList = AbstractFunction<U, V>.ErrorCheck(this.ModelState);
                returnValue = ErrorCheck(request, detailErrorList);

                if (!returnValue.Success)
                {
                    loggingUtil.EndLog(returnValue);
                    return BadRequest(returnValue);
                }

                // Execute business logic
                returnValue = await Exec(request);
                return Ok(returnValue);
            }
            catch (Exception ex)
            {
                return AbstractFunction<U, V>.GetReturnValue(returnValue, loggingUtil, ex);
            }
        }
}