using System.Security.Claims;
using OpenIddict.Abstractions;

namespace OrchidStore.API.SystemClient;

public class IdentityApiClient : IIdentityApiClient
{
    public IdentityEntity GetIdentity(ClaimsPrincipal user)
    {
        var identity = user.Identity as ClaimsIdentity;
        
        // Get id
        var id = identity!.FindFirst(OpenIddictConstants.Claims.Subject)!.Value;
        
        // Get email
        var email = identity!.FindFirst(OpenIddictConstants.Claims.Email)!.Value;
        
        // Get name
        var name = identity.FindFirst(OpenIddictConstants.Claims.Name)?.Value;
        
        // Get role
        var role = identity.FindFirst(OpenIddictConstants.Claims.Role)?.Value;
        
        // Create IdentityEntity
        var identityEntity = new IdentityEntity
        {
            UserId = id,
            Email = email,
            AccountName = name!,
            RoleName = role!,
        };
        return identityEntity;
    }
}