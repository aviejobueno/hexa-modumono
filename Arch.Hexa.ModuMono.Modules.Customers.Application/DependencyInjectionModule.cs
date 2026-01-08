using Arch.Hexa.ModuMono.Modules.Customers.Application.Mapping;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Hexa.ModuMono.Modules.Customers.Application
{
    public static class DependencyInjectionModule
    {
        public static IServiceCollection AddCustomersApplication(this IServiceCollection services)
        {

            services.AddTransient<ICustomersMapping, CustomersMapping>();

            return services;
        }
    }
}
