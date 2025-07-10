// using System.Security.Claims;
// using MediatR;
// using Microsoft.AspNetCore;
// using Microsoft.AspNetCore.Authentication;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.IdentityModel.Tokens;
// using OpenIddict.Abstractions;
// using OpenIddict.Server.AspNetCore;
// using OpenIddict.Validation.AspNetCore;
// using OrchidStore.API.Helpers;
// using OrchidStore.API.SystemClient;
// using static OpenIddict.Abstractions.OpenIddictConstants;
//
// namespace OrchidStore.API.Controllers;
//
// [ApiController]
// [Route("api/v1/[controller]")]
// public class AuthController : ControllerBase
// {
//     private readonly IOpenIddictScopeManager _scopeManager;
//     private readonly IOpenIddictTokenManager _tokenManager;
//     private readonly IMediator _mediator;
//     private readonly IIdentityApiClient _identityApiClient;
//
//     public AuthController(IMediator mediator, IOpenIddictScopeManager scopeManager, IOpenIddictTokenManager tokenManager, IIdentityApiClient identityApiClient)
//     {
//         _mediator = mediator;
//         _scopeManager = scopeManager;
//         _tokenManager = tokenManager;
//         _identityApiClient = identityApiClient;
//     }
//     
//     /// <summary>
//     /// Get role of the user based on the token.
//     /// </summary>
//     /// <returns></returns>
//     // [HttpGet]
//     // [Route("[action]")]
//     // [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
//     // public async Task<IActionResult> SelectToken()
//     // {
//     //     var identity = _identityApiClient.GetIdentity(User);
//     //
//     //     if (identity == null)
//     //     {
//     //         var response = new SelectTokenQueryResponse { Success = false };
//     //         response.SetMessage(MessageId.E11001);
//     //         return Unauthorized(response);
//     //     }
//     //
//     //     var result = await _mediator.Send(new SelectTokenQuery(identity.RoleName));
//     //     if (result.MessageId == MessageId.E11001)
//     //     {
//     //         return Unauthorized(result);
//     //     }
//     //     return Ok(result);
//     // }
//
//     /// <summary>
//     /// Exchange token
//     /// </summary>
//     /// <param name="request"></param>
//     /// <returns></returns>
//     [HttpPost("~/connect/token")]
//     [Consumes("application/x-www-form-urlencoded")]
//     [Produces("application/json")]
//     public async Task<IActionResult> Exchange([FromForm] LoginUserCommand request)
//     {
//         var openIdRequest = HttpContext.GetOpenIddictServerRequest();
//
//         // Password
//         if (openIdRequest!.IsPasswordGrantType())
//         {
//             return await TokensForPasswordGrantType(request);
//         }
//
//         // Refresh token
//         if (openIdRequest!.IsRefreshTokenGrantType())
//         {
//             return await TokensForRefreshTokenGrantType();
//         }
//
//         // Unsupported grant type
//         return BadRequest(new OpenIddictResponse
//         {
//             Error = Errors.UnsupportedGrantType
//         });
//     }
//
//     /// <summary>
//     /// Handle refresh token grant type
//     /// </summary>
//     /// <returns></returns>
//     private async Task<IActionResult> TokensForRefreshTokenGrantType()
//     {
//         try
//         {
//             // Authenticate the refresh token
//             var authenticateResult = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
//
//             if (!authenticateResult.Succeeded)
//             {
//                 return Unauthorized(new OpenIddictResponse
//                 {
//                     Error = Errors.InvalidGrant,
//                     ErrorDescription = "The refresh token is invalid."
//                 });
//             }
//
//             var claimsPrincipal = authenticateResult.Principal;
//
//             // Validate that the user still exists and is active
//             var userId = claimsPrincipal.GetClaim(Claims.Subject);
//             if (string.IsNullOrEmpty(userId))
//             {
//                 return Unauthorized(new OpenIddictResponse
//                 {
//                     Error = Errors.InvalidGrant,
//                     ErrorDescription = "The refresh token is invalid."
//                 });
//             }
//
//             // Create new identity with fresh claims
//             var identity = new ClaimsIdentity(
//                 TokenValidationParameters.DefaultAuthenticationType,
//                 Claims.Name,
//                 Claims.Role
//             );
//
//             // Copy existing claims from the refresh token
//             foreach (var claim in claimsPrincipal.Claims)
//             {
//                 // Copy essential claims
//                 switch (claim.Type)
//                 {
//                     case Claims.Subject:
//                         identity.SetClaim(Claims.Subject, claim.Value, Destinations.AccessToken);
//                         break;
//                     case Claims.Name:
//                         identity.SetClaim(Claims.Name, claim.Value, Destinations.AccessToken);
//                         break;
//                     case Claims.Email:
//                         identity.SetClaim(Claims.Email, claim.Value, Destinations.AccessToken);
//                         break;
//                     case Claims.PhoneNumber:
//                         identity.SetClaim(Claims.PhoneNumber, claim.Value, Destinations.AccessToken);
//                         break;
//                     case Claims.Role:
//                         identity.SetClaim(Claims.Role, claim.Value, Destinations.AccessToken);
//                         break;
//                     case Claims.Audience:
//                         identity.SetClaim(Claims.Audience, claim.Value, Destinations.AccessToken);
//                         break;
//                 }
//             }
//
//             // Set destinations for claims
//             identity.SetDestinations(claim =>
//             {
//                 return claim.Type switch
//                 {
//                     Claims.Subject => new[] { Destinations.AccessToken },
//                     Claims.Name => new[] { Destinations.AccessToken },
//                     Claims.Email => new[] { Destinations.AccessToken },
//                     Claims.PhoneNumber => new[] { Destinations.AccessToken },
//                     Claims.Role => new[] { Destinations.AccessToken },
//                     Claims.Audience => new[] { Destinations.AccessToken },
//                     _ => new[] { Destinations.AccessToken }
//                 };
//             });
//
//             // Create new claims principal
//             var newClaimsPrincipal = new ClaimsPrincipal(identity);
//
//             // Set scopes (preserve original scopes)
//             newClaimsPrincipal.SetScopes(claimsPrincipal.GetScopes());
//
//             // Set resources
//             newClaimsPrincipal.SetResources(await _scopeManager.ListResourcesAsync(newClaimsPrincipal.GetScopes())
//                 .ToListAsync());
//
//             // Set token lifetimes
//             newClaimsPrincipal.SetAccessTokenLifetime(TimeSpan.FromHours(1));
//             newClaimsPrincipal.SetRefreshTokenLifetime(TimeSpan.FromHours(2));
//
//             return SignIn(newClaimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
//         }
//         catch (Exception ex)
//         {
//             return BadRequest(new OpenIddictResponse
//             {
//                 Error = Errors.ServerError,
//                 ErrorDescription = "An error occurred while processing the refresh token."
//             });
//         }
//     }
//
//     /// <summary>
//     /// Generate tokens for the user
//     /// </summary>
//     /// <param name="request"></param>
//     /// <returns></returns>
//     public async Task<IActionResult> TokensForPasswordGrantType(LoginUserCommand request)
//     {
//         // Else user use password login
//         var userPasswordLogin = await _mediator.Send(request);
//         if (!userPasswordLogin.Success)
//         {
//             return Unauthorized(new OpenIddictResponse
//             {
//                 Error = Errors.InvalidRequest,
//                 ErrorDescription = userPasswordLogin.Message,
//             });
//         }
//
//         // Create claims
//         var identity = new ClaimsIdentity(
//             TokenValidationParameters.DefaultAuthenticationType,
//             Claims.Name,
//             Claims.Role
//         );
//
//         // Set claims
//         identity.SetClaim(Claims.Subject, userPasswordLogin.Response.UserId.ToString(),
//             Destinations.AccessToken);
//         identity.SetClaim(Claims.Name, userPasswordLogin.Response.FullName,
//             Destinations.AccessToken);
//         identity.SetClaim(Claims.Email, userPasswordLogin.Response.Email,
//             Destinations.AccessToken);
//         identity.SetClaim(Claims.PhoneNumber, userPasswordLogin.Response.Email,
//             Destinations.AccessToken);
//         identity.SetClaim(Claims.Role, userPasswordLogin.Response.RoleName,
//             Destinations.AccessToken);
//         identity.SetClaim(Claims.Audience, "service_client",
//             Destinations.AccessToken);
//
//         identity.SetDestinations(claim =>
//         {
//             return claim.Type switch
//             {
//                 Claims.Subject => new[] { Destinations.AccessToken },
//                 Claims.Name => new[] { Destinations.AccessToken },
//                 Claims.Email => new[] { Destinations.AccessToken },
//                 Claims.Role => new[] { Destinations.AccessToken },
//                 Claims.Audience => new[] { Destinations.AccessToken },
//                 _ => new[] { Destinations.AccessToken }
//             };
//         });
//
//         // Set scopes
//         var claimsPrincipal = new ClaimsPrincipal(identity);
//         claimsPrincipal.SetScopes(new string[]
//         {
//             Scopes.Roles,
//             Scopes.OfflineAccess,
//             Scopes.Profile,
//         });
//
//         claimsPrincipal.SetResources(await _scopeManager.ListResourcesAsync(claimsPrincipal.GetScopes()).ToListAsync());
//
//         // Set refresh token and access token
//         claimsPrincipal.SetAccessTokenLifetime(TimeSpan.FromHours(1));
//         claimsPrincipal.SetRefreshTokenLifetime(TimeSpan.FromHours(2));
//
//         return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
//     }
//
//     /// <summary>
//     /// Logout endpoint - revoke tokens properly
//     /// </summary>
//     /// <returns></returns>
//     [HttpPost("~/connect/logout")]
//     [Produces("application/json")]
//     [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
//     public async Task<IActionResult> Logout()
//     {
//         try
//         {
//             // Get the current user's claims
//             var identity = _identityApiClient.GetIdentity(User);
//
//             if (identity == null)
//             {
//                 return BadRequest(new OpenIddictResponse
//                 {
//                     Error = Errors.InvalidRequest,
//                     ErrorDescription = "Invalid user identity."
//                 });
//             }
//
//             // Get the current access token from the Authorization header
//             var accessToken = Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
//
//             if (!string.IsNullOrEmpty(accessToken))
//             {
//                 // Find and revoke the access token
//                 var token = await _tokenManager.FindByReferenceIdAsync(accessToken);
//                 if (token != null)
//                 {
//                     await _tokenManager.TryRevokeAsync(token);
//                 }
//             }
//
//             // Revoke all active tokens for this user
//             var userId = identity.UserId;
//             await foreach (var token in _tokenManager.FindBySubjectAsync(userId))
//             {
//                 // Revoke both access tokens and refresh tokens
//                 await _tokenManager.TryRevokeAsync(token);
//             }
//
//             // Sign out the user from the current context
//             await HttpContext.SignOutAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
//
//             var request = HttpContext.GetOpenIddictServerRequest();
//
//             return Ok(new
//             {
//                 Success = true,
//                 Message = "Logged out successfully. All tokens have been revoked."
//             });
//         }
//         catch (Exception ex)
//         {
//             return BadRequest(new OpenIddictResponse
//             {
//                 Error = Errors.ServerError,
//                 ErrorDescription = "An error occurred while logging out."
//             });
//         }
//     }
// }