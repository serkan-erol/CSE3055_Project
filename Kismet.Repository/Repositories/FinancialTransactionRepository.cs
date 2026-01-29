using Dapper;
using Kismet.Core.Data;
using Kismet.Entities.DTOs;
using Kismet.Entities.Enums;
using Kismet.Repository.Helpers;
using Kismet.Repository.Interfaces;

namespace Kismet.Repository.Repositories;

public class FinancialTransactionRepository : IFinancialTransactionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ICustomerRepository _customerRepository;
    private readonly IBillingRepository _billingRepository;
    private readonly IOrderRepository _orderRepository;

    public FinancialTransactionRepository(IDbConnectionFactory connectionFactory, 
                                          ICustomerRepository customerRepository, 
                                          IBillingRepository billingRepository,
                                          IOrderRepository orderRepository)
    {
        _connectionFactory = connectionFactory;
        _customerRepository = customerRepository;
        _billingRepository = billingRepository;
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// Get all FT for a customer
    /// </summary>
    public async Task<IReadOnlyList<FTResponseToCustomerDto>> GetFTForCustomerAsync(int customerId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        // Build the query dynamically based on the parameters
        var query = SqlQueries.FinancialTransaction.GetFTBaseCustomer + " WHERE ft.CustomerID = @CustomerID ORDER BY ft.FTransactionID";

        // Execute the query and return the FTs
        var fts = await connection.QueryAsync<FTResponseToCustomerDto>(
            new CommandDefinition(query, 
                new { CustomerID = customerId }, 
                cancellationToken: cancellationToken));
        return fts.ToList().AsReadOnly();
    }

    /// <summary>
    /// Get a Customer's all FTs for employees to see
    /// </summary>
    public async Task<IReadOnlyList<FTResponseToEmployeeDto>> GetFTByCustomerIdForEmployeeAsync(int customerId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        // Build the query dynamically based on the parameters
        var query = SqlQueries.FinancialTransaction.GetFTBaseEmployee + " WHERE ft.CustomerID = @CustomerID ORDER BY ft.FTransactionID";

        // Execute the query and return the FTs
        var fts = await connection.QueryAsync<FTResponseToEmployeeDto>(
            new CommandDefinition(query, new { CustomerID = customerId }, cancellationToken: cancellationToken));
        return fts.ToList().AsReadOnly();
    }

    /// <summary>
    /// Get ALL FT for employees to see
    /// </summary>
    public async Task<IReadOnlyList<FTResponseToEmployeeDto>> GetFTForEmployeeAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        // Build the query dynamically based on the parameters
        var query = SqlQueries.FinancialTransaction.GetFTBaseEmployee + " ORDER BY ft.FTransactionID";

        // Execute the query and return the FTs
        var fts = await connection.QueryAsync<FTResponseToEmployeeDto>(
            new CommandDefinition(query, cancellationToken: cancellationToken));
        return fts.ToList().AsReadOnly();
    }

        /// <summary>
    /// Get a FT by ID for a customer
    /// </summary>
    public async Task<FTResponseToCustomerDto> GetFTByIdForCustomerAsync(int customerId, int fTransactionId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        // Build the query dynamically based on the parameters
            var query = SqlQueries.FinancialTransaction.GetFTBaseCustomer + " WHERE ft.FTransactionID = @FTransactionID AND ft.CustomerID = @CustomerID";

        // Execute the query and return the FT
        var ft = await connection.QueryFirstOrDefaultAsync<FTResponseToCustomerDto>(
            new CommandDefinition(query, new { FTransactionID = fTransactionId, CustomerID = customerId }, cancellationToken: cancellationToken));
        return ft ?? throw new InvalidOperationException("FT not found");
    }

    /// <summary>
    /// Get a FT by ID for an employee
    /// </summary>
    public async Task<FTResponseToEmployeeDto> GetFTByIdForEmployeeAsync(int fTransactionId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        // Build the query dynamically based on the parameters
        var query = SqlQueries.FinancialTransaction.GetFTBaseEmployee + " WHERE ft.FTransactionID = @FTransactionID";

        // Execute the query and return the FT
        var ft = await connection.QueryFirstOrDefaultAsync<FTResponseToEmployeeDto>(
            new CommandDefinition(query, new { FTransactionID = fTransactionId }, cancellationToken: cancellationToken));
        return ft ?? throw new InvalidOperationException("FT not found");
    }

    /// <summary>
    /// Get the payment status of a FT
    /// </summary>
    public async Task<PaymentStatus> GetFTPaymentStatusAsync(int fTransactionId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        // Get the payment status by ID
        var paymentStatus = await connection.QueryFirstOrDefaultAsync<PaymentStatus>(
            new CommandDefinition(SqlQueries.FinancialTransaction.GetFTPaymentStatusById, new { FTransactionID = fTransactionId }, cancellationToken: cancellationToken));

        // Return the payment status
        return paymentStatus;
    }

    /// <summary>
    /// Get Display Name of FT Payment Status
    /// </summary>
    public async Task<string> GetFTPaymentStatusDisplayNameAsync(int fTransactionId, CancellationToken cancellationToken = default)
    {
        var ftPaymentStatus = await GetFTPaymentStatusAsync(fTransactionId, cancellationToken);
        return ftPaymentStatus.GetDisplayName();
    }
    
    /// <summary>
    /// Create a new FT for customer and order
    /// </summary>
    public async Task<FTResponseToCustomerDto> CreateFTAsync(CreateFTDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var fTransactionId = await connection.QuerySingleAsync<int>(
            new CommandDefinition(SqlQueries.FinancialTransaction.InsertFT, dto, cancellationToken: cancellationToken));
        
        // Use the existing method to retrieve the FT
        return await GetFTByIdForCustomerAsync(dto.CustomerID, fTransactionId, cancellationToken);
    }

    /// <summary>
    /// Update the TotalPaid field of a FT
    /// </summary>
    public async Task<FTResponseToCustomerDto> UpdateFTTotalPaidAsync(int customerId, int fTransactionId, UpdateFTTotalPaidDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(SqlQueries.FinancialTransaction.UpdateFTTotalPaid, dto, cancellationToken: cancellationToken));

        var ft = await GetFTByIdForCustomerAsync(customerId, fTransactionId, cancellationToken);

        // Return the updated FT
        return ft;
    }

    /// <summary>
    /// Update the description of a FT
    /// </summary>
    public async Task<FTResponseToEmployeeDto> UpdateFTDescriptionAsync(UpdateFTDescriptionDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        await connection.ExecuteAsync(new CommandDefinition(SqlQueries.FinancialTransaction.UpdateFTDescription, dto, cancellationToken: cancellationToken));

        var ft = await GetFTByIdForEmployeeAsync(dto.FTransactionID, cancellationToken);

        // Return the updated FT
        return ft;
    }

    /// <summary>
    /// Find Suitable Billing Entry for a FT
    /// </summary>
    public async Task<BillingResponseToEmployeeDto?> FindSuitableBillingEntryForFTAsync(CreateFTDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        // Execute the query and return the latest suitable Billing entry (only one row due to TOP 1)
        var suitableBillingEntry = await connection.QueryFirstOrDefaultAsync<BillingResponseToEmployeeDto>(
            new CommandDefinition(SqlQueries.FinancialTransaction.FindSuitableBillingEntryForFT, dto, cancellationToken: cancellationToken));

        return suitableBillingEntry;
    }

    /// <summary>
    /// Check if a Billing entry is suitable for a FT
    /// </summary>
    public async Task<bool> IsBillingEntrySuitableForFTAsync(CreateFTDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var query = SqlQueries.Billing.GetBillingBaseEmployee + " WHERE b.CustomerID = @CustomerID AND b.BillingType = @TransactionType AND b.BillingDate >= @TransactionDate";
        var billingEntry = await connection.QueryFirstOrDefaultAsync<BillingResponseToEmployeeDto>(
            new CommandDefinition(query, dto, cancellationToken: cancellationToken));
        return billingEntry is not null ? true : false;
    }
}