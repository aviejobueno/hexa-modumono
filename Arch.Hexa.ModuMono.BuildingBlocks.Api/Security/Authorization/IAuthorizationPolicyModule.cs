using Microsoft.AspNetCore.Authorization;

namespace Arch.Hexa.ModuMono.BuildingBlocks.Api.Security.Authorization;

public interface IAuthorizationPolicyModule
{
    void AddPolicies(AuthorizationOptions options);
}
