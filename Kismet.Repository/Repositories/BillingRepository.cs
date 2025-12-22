using Dapper;
using Kismet.Core.Data;
using Kismet.Entities.DTOs;
using Kismet.Entities.Enums;
using Kismet.Repository.Helpers;
using Kismet.Repository.Interfaces;

namespace Kismet.Repository.Repositories;

public class BillingRepository : IBillingRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ICustomerRepository _customerRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public BillingRepository(IDbConnectionFactory connectionFactory, 
                             ICustomerRepository customerRepository, 
                             IEmployeeRepository employeeRepository)
    {
        _connectionFactory = connectionFactory;
        _customerRepository = customerRepository;
        _employeeRepository = employeeRepository;
    }

    /// <summary>
    /// Get all billing for a customer
    /// </summary>
    public async Task<IReadOnlyList<BillingResponseToCustomerDto>> GetBillingForCustomerAsync(int customerId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        // Build the query dynamically based on the parameters
        var query = SqlQueries.Billing.GetBillingBaseCustomer + " WHERE b.CustomerID = @CustomerID ORDER BY b.BillingID";

        // Execute the query and return the billings
        var billings = await connection.QueryAsync<BillingResponseToCustomerDto>(
            new CommandDefinition(query, 
                new { CustomerID = customerId }, 
                cancellationToken: cancellationToken));
        return billings.ToList().AsReadOnly();
    }

    /// <summary>
    /// Get ALL billing for employees to see
    /// </summary>
    public async Task<IReadOnlyList<BillingResponseToEmployeeDto>> GetBillingForEmployeeAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        // Build the query dynamically based on the parameters
        var query = SqlQueries.Billing.GetBillingBaseEmployee + " ORDER BY b.BillingID";

        // Execute the query and return the billings
        var billings = await connection.QueryAsync<BillingResponseToEmployeeDto>(
            new CommandDefinition(query, cancellationToken: cancellationToken));
        return billings.ToList().AsReadOnly();
    }

        /// <summary>
    /// Get a billing by ID for a customer
    /// </summary>
    public async Task<BillingResponseToCustomerDto> GetBillingByIdForCustomerAsync(int customerId, int billingId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        // Build the query dynamically based on the parameters
        var query = SqlQueries.Billing.GetBillingBaseCustomer + " WHERE b.BillingID = @BillingID AND b.CustomerID = @CustomerID";

        // Execute the query and return the billing
        var billing = await connection.QueryFirstOrDefaultAsync<BillingResponseToCustomerDto>(
            new CommandDefinition(query, new { BillingID = billingId, CustomerID = customerId }, cancellationToken: cancellationToken));
        return billing ?? throw new InvalidOperationException("Billing not found");
    }

    /// <summary>
    /// Get a billing by ID for an employee
    /// </summary>
    public async Task<BillingResponseToEmployeeDto> GetBillingByIdForEmployeeAsync(int billingId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        // Build the query dynamically based on the parameters
        var query = SqlQueries.Billing.GetBillingBaseEmployee + " WHERE b.BillingID = @BillingID";

        // Execute the query and return the billing
        var billing = await connection.QueryFirstOrDefaultAsync<BillingResponseToEmployeeDto>(
            new CommandDefinition(query, new { BillingID = billingId }, cancellationToken: cancellationToken));
        return billing ?? throw new InvalidOperationException("Billing not found");
    }
    
    /// <summary>
    /// Create a new billing for customer and financial transaction
    /// //aaa TotalDue should come from Financial Transaction tables
    /// </summary>
    public async Task<BillingResponseToCustomerDto> CreateBillingAsync(CreateBillingDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var billingId = await connection.QuerySingleAsync<int>(
            new CommandDefinition(SqlQueries.Billing.InsertBilling, dto, cancellationToken: cancellationToken));
        
        // Use the existing method to retrieve the billing, just like Customer/Employee do
        return await GetBillingByIdForCustomerAsync(dto.CustomerID, billingId, cancellationToken);
    }

    /// <summary>
    /// Update the type of a billing
    /// </summary>
    public async Task<BillingResponseToEmployeeDto> UpdateBillingTypeAsync(UpdateBillingTypeDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(SqlQueries.Billing.UpdateBillingType, dto, cancellationToken: cancellationToken));

        var billing = await GetBillingByIdForEmployeeAsync(dto.BillingID, cancellationToken);

        // Return the updated billing
        return billing;
    }

    /// <summary>
    /// Update the total due and total paid of a billing
    /// </summary>
    public async Task<BillingResponseToCustomerDto> UpdateBillingAsync(int customerId, int billingId, /*int paymentId,*/ UpdateBillingDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        dto.BillingID = billingId;

        if (dto.TotalDue is not null && dto.TotalDue > 0)
        {
            // Update the billing total due
            await connection.ExecuteAsync(
                new CommandDefinition(SqlQueries.Billing.UpdateBillingTotalDue, dto, cancellationToken: cancellationToken));
        }
        
        if (dto.TotalPaid is not null && dto.TotalPaid > 0)
        {
            // Update the billing total paid
            await connection.ExecuteAsync(
                new CommandDefinition(SqlQueries.Billing.UpdateBillingTotalPaid, dto, cancellationToken: cancellationToken));
        }

        var billing = await GetBillingByIdForCustomerAsync(customerId, billingId, cancellationToken);

        // Return the updated billing
        return billing;
    }

    /// <summary>
    /// Update the payment terms of a billing
    /// </summary>
    public async Task<BillingResponseToCustomerDto> UpdateBillingPaymentTermsAsync(UpdateBillingPaymentTermsDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        // Get the customer ID from the billing
        var billingForCustomer = await GetBillingByIdForEmployeeAsync(dto.BillingID, cancellationToken);
        var customerId = billingForCustomer.CustomerID;

        // Update the billing payment terms
        await connection.ExecuteAsync(
            new CommandDefinition(SqlQueries.Billing.UpdateBillingPaymentTerms, dto, cancellationToken: cancellationToken));
        
        var billing = await GetBillingByIdForCustomerAsync(customerId, dto.BillingID, cancellationToken);
        
        // Return the updated billing
        return billing;
    }

    /// <summary>
    /// Get the status of a billing
    /// </summary>
    public async Task<PaymentStatus> GetBillingStatusByIdAsync(int billingId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        // Get the billing status by ID
        var billingStatus = await connection.QueryFirstOrDefaultAsync<PaymentStatus>(
            new CommandDefinition(SqlQueries.Billing.GetBillingStatusById, new { BillingID = billingId }, cancellationToken: cancellationToken));

        return billingStatus;
    }

    /// <summary>
    /// Get the display name of a billing status by ID
    /// </summary>
    public async Task<string> GetBillingStatusDisplayNameByIdAsync(int billingId, CancellationToken cancellationToken = default)
    {
        var billingStatus = await GetBillingStatusByIdAsync(billingId, cancellationToken);
        return billingStatus.GetDisplayName();
    }
}