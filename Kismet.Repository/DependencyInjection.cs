using Kismet.Repository.Interfaces;
using Kismet.Repository.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Kismet.Repository;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IBillingRepository, BillingRepository>();
        services.AddScoped<IFinancialTransactionRepository, FinancialTransactionRepository>();
        //aaa services.AddScoped<ITreasuryRepository, TreasuryRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        //aaa services.AddScoped<IShipmentRepository, ShipmentRepository>();
        //aaa services.AddScoped<IFinancialTransactionRepository, FinancialTransactionRepository>();
        services.AddScoped<ITreasuryRepository, TreasuryRepository>();
        services.AddScoped<ICustomerPaymentRepository, CustomerPaymentRepository>();
        //aaa services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<IFabricRepository, FabricRepository>();
        //aaa services.AddScoped<IUnitPriceRepository, UnitPriceRepository>();
        services.AddScoped<IBatchRepository, BatchRepository>();
        services.AddScoped<IViewRepository, ViewRepository>();
        return services;
    }
}

