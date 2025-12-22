using Kismet.Repository.Interfaces;
using Kismet.Repository.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Kismet.Repository;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IBillingRepository, BillingRepository>();
        //aaa services.AddScoped<IFinancialTransactionRepository, FinancialTransactionRepository>();
        //aaa services.AddScoped<ITreasuryRepository, TreasuryRepository>();
        //aaa services.AddScoped<IPaymentRepository, PaymentRepository>();
        //aaa services.AddScoped<IShipmentRepository, ShipmentRepository>();
        //aaa services.AddScoped<IFabricRepository, FabricRepository>();
        //aaa services.AddScoped<IUnitPriceRepository, UnitPriceRepository>();
        //aaa services.AddScoped<IBatchRepository, BatchRepository>();
        return services;
    }
}

