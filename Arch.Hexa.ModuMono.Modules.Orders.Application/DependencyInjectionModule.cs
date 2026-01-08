using Arch.Hexa.ModuMono.Modules.Orders.Application.Mapping;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Hexa.ModuMono.Modules.Orders.Application;

public static class DependencyInjectionModule
{
    public static IServiceCollection AddOrdersApplication(this IServiceCollection services)
    {

        services.AddTransient<IOrdersMapping, OrdersMapping>();

        return services;
    }
}

