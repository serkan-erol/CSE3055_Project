using Dapper;
using Kismet.Repository.Helpers;
using Kismet.Repository.Interfaces;
using Kismet.Repository.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Kismet.Repository;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Register Dapper type handlers for DateOnly
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new NullableDateOnlyTypeHandler());
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerPaymentRepository, CustomerPaymentRepository>();
        services.AddScoped<IBillingRepository, BillingRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IFinancialTransactionRepository, FinancialTransactionRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ITreasuryRepository, TreasuryRepository>();
        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<IFabricRepository, FabricRepository>();
        services.AddScoped<IBatchRepository, BatchRepository>();
        services.AddScoped<IViewRepository, ViewRepository>();
        return services;
    }
}

