using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Arch.Hexa.ModuMono.BuildingBlocks.Api.Security.Authorization;
public static class AuthorizationModuleRegistration
{
    public static IServiceCollection AddAuthorizationModules(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var moduleTypes = assemblies
            .SelectMany(a => a.DefinedTypes)
            .Where(t => !t.IsAbstract && typeof(IAuthorizationPolicyModule).IsAssignableFrom(t))
            .Select(t => t.AsType())
            .ToList();

        foreach (var type in moduleTypes)
            services.AddSingleton(typeof(IAuthorizationPolicyModule), type);

        services.AddSingleton<IConfigureOptions<AuthorizationOptions>, AuthorizationOptionsFromModulesConfigurer>();

        return services;
    }
}