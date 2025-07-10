using System.Security.Claims;

namespace OrchidStore.API.SystemClient;

public interface IIdentityApiClient
{
    public IdentityEntity? GetIdentity(ClaimsPrincipal user);
}