using BackEnd.Utils.Const;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NLog;
using OrchidStore.API.SystemClient;
using OrchidStore.Application.Features;
using OrchidStore.Application.LogConfig;

namespace OrchidStore.API.Controllers
{
    public abstract class AbstractApiAsyncController<T, U, V> : ControllerBase
        where T : AbstractApiRequest
        where U : AbstractApiResponse<V>, new()
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
        /// API Client to extract identity info
        /// </summary>
        protected IIdentityApiClient _identityApiClient;

        /// <summary>
        /// Identity extracted from JWT
        /// </summary>
        protected IdentityEntity _identityEntity;

        /// <summary>
        /// Template Method
        /// </summary>
        protected async Task<IActionResult> ProcessRequest(T request, Logger logger, U returnValue)
        {
            // et identity from current user
            _identityEntity = _identityApiClient.GetIdentity(User);

            var loggingUtil = new LoggingUtil(logger, _identityEntity?.Email!);
            loggingUtil.StartLog(request);

            // Validate identity
            if (_identityEntity == null)
            {
                returnValue.Success = false;
                returnValue.SetMessage(MessageId.E11006);
                loggingUtil.FatalLog("Authenticated but user info is missing.");
                loggingUtil.EndLog(returnValue);
                return Unauthorized(returnValue);
            }

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
}