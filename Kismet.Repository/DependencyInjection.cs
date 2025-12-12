using Kismet.Repository.Interfaces;
using Kismet.Repository.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Kismet.Repository;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        return services;
    }
}

