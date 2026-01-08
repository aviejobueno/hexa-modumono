using Arch.Hexa.ModuMono.BuildingBlocks.Api.Helpers;
using Arch.Hexa.ModuMono.BuildingBlocks.Api.Security.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Arch.Hexa.ModuMono.Customers.RestApi.Security;

public sealed class CustomersAuthorizationModule : IAuthorizationPolicyModule
{
    public void AddPolicies(AuthorizationOptions options)
    {
        options.AddPolicy(CustomersPolicies.Read, policy =>
            policy.RequireAssertion(ctx =>
                AuthorizationHelper.HasScope(ctx.User, CustomersScopes.Read) ||
                ctx.User.IsInRole(CustomersRoles.Reader)));

        options.AddPolicy(CustomersPolicies.Write, policy =>
            policy.RequireAssertion(ctx =>
                AuthorizationHelper.HasScope(ctx.User, CustomersScopes.Write) ||
                ctx.User.IsInRole(CustomersRoles.Writer)));
    }
}


//Este enfoque(policies por módulo + auto-registro) está pensado para que tu API haga:
//Autenticación: validar el JWT access token emitido por Azure Entra ID(firma, issuer, audience, expiración) y construir HttpContext.User.
//    Autorización: decidir si dejas pasar usando claims del token:scp para scopes(delegated permissions)roles para app roles
//Tus OrdersAuthorizationModule / CustomersAuthorizationModule solo definen la lógica de autorización(qué policy exige qué scopes/roles).
//La autenticación se configura una vez con AddMicrosoftIdentityWebApi(...) (o AddJwtBearer(...)).

//En resumen:
//El JWT lo emite Entra.
//    El frontend React lo manda en Authorization: Bearer....
//    Tu API valida el token(AuthN) y luego aplica las policies por módulo(AuthZ).