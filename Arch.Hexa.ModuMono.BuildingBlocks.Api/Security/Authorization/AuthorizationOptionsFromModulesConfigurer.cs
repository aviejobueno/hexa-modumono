using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Arch.Hexa.ModuMono.BuildingBlocks.Api.Security.Authorization;

public sealed class AuthorizationOptionsFromModulesConfigurer(
    IEnumerable<IAuthorizationPolicyModule> modules)
    : IConfigureOptions<AuthorizationOptions>
{
    public void Configure(AuthorizationOptions options)
    {
        foreach (var module in modules)
            module.AddPolicies(options);
    }
}