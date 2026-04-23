using Dapper;
using Kismet.Core.Data;
using Kismet.Entities.DTOs;
using Kismet.Entities.Enums;
using Kismet.Repository.Helpers;
using Kismet.Repository.Interfaces;

namespace Kismet.Repository.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IFinancialTransactionRepository _financialTransactionRepository;

    public PaymentRepository(IDbConnectionFactory connectionFactory, 
                             IFinancialTransactionRepository financialTransactionRepository)
    {
        _connectionFactory = connectionFactory;
        _financialTransactionRepository = financialTransactionRepository;
    }

    /// <summary>
    /// Get a Payment by ID
    /// </summary>
    public async Task<PaymentResponseDto> GetPaymentByIdAsync(int paymentId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        // Build the query dynamically based on the parameters
        var query = SqlQueries.Payment.GetAllPayments + " WHERE p.PaymentID = @PaymentID";

        // Execute the query and return the payment
        var payment = await connection.QueryFirstOrDefaultAsync<PaymentResponseDto>(
            new CommandDefinition(query, new { PaymentID = paymentId }, cancellationToken: cancellationToken));
        return payment ?? throw new InvalidOperationException("Payment not found");
    }

    /// <summary>
    /// Get all Payments for a customer
    /// </summary>
    public async Task<IReadOnlyList<PaymentResponseDto>> GetAllCustomerPaymentsAsync(int customerId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        // Find all the FTs for the customer
        var fts = await _financialTransactionRepository.GetFTForCustomerAsync(customerId, cancellationToken);

        // List to store all the payments
        var allPayments = new List<PaymentResponseDto>();

        foreach (var ft in fts)
        {
            // Build the query dynamically based on the parameters for each FT
            var query = SqlQueries.Payment.GetAllPayments + " WHERE p.FTransactionID = @FTransactionID ORDER BY p.PaymentID";

            // Execute the query and return the payments
            var ftPayments = await connection.QueryAsync<PaymentResponseDto>(
                new CommandDefinition(query, new { FTransactionID = ft.FTransactionID }, cancellationToken: cancellationToken));
            allPayments.AddRange(ftPayments);
        }
        return allPayments;
    }

    /// <summary>
    /// Get a Payment by ID for a customer
    /// </summary>
    public async Task<PaymentResponseDto> GetCustomerPaymentByIdAsync(int customerId, int paymentId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        // Build the query dynamically based on the parameters
        var query = SqlQueries.Payment.GetAllPayments + " WHERE p.PaymentID = @PaymentID AND p.CustomerID = @CustomerID";

        // Execute the query and return the payment
        var payment = await connection.QueryFirstOrDefaultAsync<PaymentResponseDto>(
            new CommandDefinition(query, new { PaymentID = paymentId, CustomerID = customerId }, cancellationToken: cancellationToken));
        return payment ?? throw new InvalidOperationException("Payment not found");
    }

    
    /// <summary>
    /// Create a new Payment for a FT
    /// </summary>
    public async Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var paymentId = await connection.QuerySingleAsync<int>(
            new CommandDefinition(SqlQueries.Payment.CreatePayment, dto, cancellationToken: cancellationToken));
        
        // Use the existing method to retrieve the payment
        return await GetPaymentByIdAsync(paymentId, cancellationToken);
    }
}