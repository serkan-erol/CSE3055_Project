using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kismet.Bussiness.Interfaces;
using Kismet.Core.Common;
using Kismet.Core.Data;
using Kismet.Entities.DTOs;
using Kismet.Entities.Models;
using Kismet.Repository.Interfaces;

namespace Kismet.Bussiness.Services;

public class CustomerService : ICustomerService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(IDbConnectionFactory connectionFactory, ICustomerRepository customerRepository)
    {
        _connectionFactory = connectionFactory;
        _customerRepository = customerRepository;
    }

    public async Task<Result<IEnumerable<Customer>>> GetCustomersAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var customers = await _customerRepository.GetAllAsync(cancellationToken);
        return Result<IEnumerable<Customer>>.Ok(customers);
    }

    public async Task<Result<CustomerDto>> CreateCustomerAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var customer = new Customer
        {
            CustomerName = dto.CustomerName,
            CustomerType = dto.CustomerType,
            ReliabilityStatus = dto.ReliabilityStatus,
            PaymentType = dto.PaymentType,
            ContactInfo = dto.ContactInfo
        };

        var created = await _customerRepository.AddAsync(customer, cancellationToken);

        var resultDto = new CustomerDto
        {
            CustomerId = created.CustomerId,
            CustomerName = created.CustomerName,
            CustomerType = created.CustomerType,
            ReliabilityStatus = created.ReliabilityStatus,
            PaymentType = created.PaymentType,
            ContactInfo = created.ContactInfo
        };

        return Result<CustomerDto>.Ok(resultDto, "Customer created successfully.");
    }

    public async Task<Result> DeleteCustomerAsync(DeleteCustomerDto dto, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var deleted = await _customerRepository.DeleteAsync(dto.CustomerId, cancellationToken);
        if (!deleted)
        {
            return Result.Fail("Customer not found.");
        }

        return Result.Ok("Customer deleted successfully.");
    }
}

