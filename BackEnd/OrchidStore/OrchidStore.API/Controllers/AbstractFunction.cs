using System.Text.RegularExpressions;
using BackEnd.Utils;
using BackEnd.Utils.Const;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OrchidStore.Application.Features;
using OrchidStore.Application.LogConfig;
using LoggingUtil = OrchidStore.Application.LogConfig.LoggingUtil;

namespace OrchidStore.API.Controllers;

public class AbstractFunction<U, V>
    where U : AbstractApiResponse<V>
{
    /// <summary>
    /// Return value
    /// </summary>
    public static IActionResult GetReturnValue(U returnValue, LoggingUtil loggingUtil, Exception e)
    {
        int statusCode;

        switch (e)
        {
            case AggregateException:
                loggingUtil.FatalLog($"Report API connection error: {e.Message}");
                returnValue.SetMessage(MessageId.E99001);
                statusCode = StatusCodes.Status503ServiceUnavailable;
                break;

            case DbUpdateConcurrencyException:
                loggingUtil.ErrorLog($"Concurrency error: {e.Message}");
                returnValue.SetMessage(MessageId.E99002);
                statusCode = StatusCodes.Status409Conflict;
                break;

            case InvalidOperationException:
                if (e.InnerException?.HResult == -2146233088)
                {
                    loggingUtil.ErrorLog($"Concurrency conflict: {e.Message}");
                    returnValue.SetMessage(MessageId.E99002);
                    statusCode = StatusCodes.Status409Conflict;
                }
                else
                {
                    loggingUtil.FatalLog($"System error: {e.Message} {e.StackTrace} {e.InnerException}");
                    returnValue.SetMessage(MessageId.E99999);
                    statusCode = StatusCodes.Status500InternalServerError;
                }

                break;

            case PostgresException ex:
                if (ex.SqlState == "57014")
                {
                    loggingUtil.ErrorLog($"PostgresSQL timeout error: {ex.Message} {ex.StackTrace}");
                    returnValue.SetMessage(MessageId.E99003);
                    statusCode = StatusCodes.Status504GatewayTimeout;
                }
                else if (ex.SqlState == "42P01")
                {
                    loggingUtil.ErrorLog($"Schema/view changed during execution: {ex.Message} {ex.StackTrace}");
                    returnValue.SetMessage(MessageId.E99004);
                    statusCode = StatusCodes.Status400BadRequest;
                }
                else
                {
                    loggingUtil.ErrorLog($"PostgresSQL system error: {ex.Message} {ex.StackTrace}");
                    returnValue.SetMessage(MessageId.E99005);
                    statusCode = StatusCodes.Status500InternalServerError;
                }

                break;

            default:
                loggingUtil.ErrorLog($"Unhandled exception: {e.Message} {e.StackTrace} {e.InnerException}");
                returnValue.SetMessage(MessageId.E99999);
                statusCode = StatusCodes.Status500InternalServerError;
                break;
        }

        returnValue.Success = false;
        loggingUtil.EndLog(returnValue);

        return new ObjectResult(returnValue)
        {
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Error check
    /// </summary>
    /// <param name="modelState"></param>
    /// <returns></returns>
    public static List<DetailError> ErrorCheck(ModelStateDictionary modelState)
    {
        var detailErrorList = new List<DetailError>();

        // If there is no error, return
        if (modelState.IsValid)
            return detailErrorList;

        foreach (var entry in modelState)
        {
            var key = entry.Key;
            var modelStateEntity = entry.Value;

            if (modelStateEntity == null || modelStateEntity.ValidationState == ModelValidationState.Valid)
                continue;

            // Remove the prefix "Value." from the key
            var keyReplace = Regex.Replace(key, @"^Value\.", "");
            keyReplace = Regex.Replace(keyReplace, @"^Value\[\d+\]\.", "");

            // Get error message
            var errorMessage = string.Join("; ", modelStateEntity.Errors.Select(e => e.ErrorMessage));

            var detailError = new DetailError();
            Match matchesKey;

            // Extract information from the key in the structure: object[index].property
            if ((matchesKey = new Regex(@"^(.*?)\[(\d+)\]\.(.*?)$").Match(keyReplace)).Success)
            {
                // In the case of a list
                detailError.field = matchesKey.Groups[1].Value;
            }
            else
            {
                // In the case of a single item
                detailError.field = keyReplace.Split('.').LastOrDefault();
            }

            // Convert the field name to lowercase
            detailError.field = StringUtil.ToLowerCase(detailError.field);

            // Set the error message
            detailError.ErrorMessage = errorMessage;

            detailErrorList.Add(detailError);
        }

        return detailErrorList;
    }
}