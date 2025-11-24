using Kismet.Bussiness.Interfaces;
using Kismet.Bussiness.Services;
using Kismet.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Kismet.Bussiness;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddRepositories();
        services.AddScoped<ICustomerService, CustomerService>();
        return services;
    }
}

