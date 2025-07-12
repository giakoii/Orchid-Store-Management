using System.Security.Claims;

namespace OrchidStore.Application.Logics;

public interface IIdentityService
{ 
    IdentityEntity? GetIdentity(ClaimsPrincipal user);
    
    IdentityEntity GetCurrentUser();
}