using System.Security.Claims;

namespace Arch.Hexa.ModuMono.BuildingBlocks.Api.Helpers;

public static class AuthorizationHelper
{
    public static bool HasScope(ClaimsPrincipal user, string scope)
    {
        var raw =
            user.FindFirst("scp")?.Value ??
            user.FindFirst("http://schemas.microsoft.com/identity/claims/scope")?.Value ??
            user.FindFirst("scope")?.Value ??
            string.Empty;

        return raw.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(scope);
    }

}