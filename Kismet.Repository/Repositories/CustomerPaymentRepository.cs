using Dapper;
using Kismet.Core.Data;
using Kismet.Entities.DTOs;
using Kismet.Repository.Interfaces;
using Kismet.Repository.Helpers;
using Microsoft.Extensions.Logging;

namespace Kismet.Repository.Repositories;

public class CustomerPaymentRepository : ICustomerPaymentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<CustomerPaymentRepository> _logger;

    public CustomerPaymentRepository(IDbConnectionFactory connectionFactory, ILogger<CustomerPaymentRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    // Saved Payment Methods
    public async Task<IReadOnlyList<SavedPaymentMethodResponseDto>> GetSavedPaymentMethodsByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        var query = SqlQueries.CustomerPayment.GetSavedPaymentMethodsByCustomerId;
        
        var paymentMethods = await connection.QueryAsync<SavedPaymentMethodResponseDto>(
            new CommandDefinition(query, new { CustomerID = customerId }, cancellationToken: cancellationToken));
        
        return paymentMethods.ToList().AsReadOnly();
    }

    public async Task<SavedPaymentMethodResponseDto?> GetSavedPaymentMethodByIdAsync(int spmId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        var query = SqlQueries.CustomerPayment.GetSavedPaymentMethodById;
        
        var paymentMethod = await connection.QuerySingleOrDefaultAsync<SavedPaymentMethodResponseDto>(
            new CommandDefinition(query, new { SPMID = spmId }, cancellationToken: cancellationToken));
        
        return paymentMethod;
    }

    public async Task<SavedPaymentMethodResponseDto> CreateSavedPaymentMethodAsync(CreateSavedPaymentMethodDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        try
        {
            var query = SqlQueries.CustomerPayment.CreateSavedPaymentMethod;
            
            var paymentMethod = await connection.QuerySingleAsync<SavedPaymentMethodResponseDto>(
                new CommandDefinition(
                    query,
                    new
                    {
                        dto.CustomerID,
                        dto.CardNumber,
                        dto.CardType,
                        dto.CardExpirationDate,
                        dto.RecordExpirationDate
                    },
                    cancellationToken: cancellationToken));

            _logger.LogInformation("Created saved payment method {SPMID} for customer {CustomerID}", 
                paymentMethod.SPMID, dto.CustomerID);
            
            return paymentMethod;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating saved payment method for customer {CustomerID}", dto.CustomerID);
            throw;
        }
    }

    public async Task<SavedPaymentMethodResponseDto> UpdateSavedPaymentMethodAsync(int spmId, UpdateSavedPaymentMethodDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        try
        {
            var query = SqlQueries.CustomerPayment.UpdateSavedPaymentMethod;
            
            var paymentMethod = await connection.QuerySingleAsync<SavedPaymentMethodResponseDto>(
                new CommandDefinition(
                    query,
                    new
                    {
                        SPMID = spmId,
                        dto.CardNumber,
                        dto.CardType,
                        dto.CardExpirationDate,
                        dto.RecordExpirationDate
                    },
                    cancellationToken: cancellationToken));

            _logger.LogInformation("Updated saved payment method {SPMID}", spmId);
            
            return paymentMethod;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating saved payment method {SPMID}", spmId);
            throw;
        }
    }

    public async Task<bool> DeleteSavedPaymentMethodAsync(int spmId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        try
        {
            var query = SqlQueries.CustomerPayment.DeleteSavedPaymentMethod;
            
            var result = await connection.ExecuteAsync(
                new CommandDefinition(query, new { SPMID = spmId }, cancellationToken: cancellationToken));

            if (result > 0)
            {
                _logger.LogInformation("Deleted saved payment method {SPMID}", spmId);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting saved payment method {SPMID}", spmId);
            throw;
        }
    }

    // Saved Bank Information
    public async Task<IReadOnlyList<SavedBankInformationResponseDto>> GetSavedBankInformationByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        var query = SqlQueries.CustomerPayment.GetSavedBankInformationByCustomerId;
        
        var bankInfo = await connection.QueryAsync<SavedBankInformationResponseDto>(
            new CommandDefinition(query, new { CustomerID = customerId }, cancellationToken: cancellationToken));
        
        return bankInfo.ToList().AsReadOnly();
    }

    public async Task<SavedBankInformationResponseDto?> GetSavedBankInformationByIdAsync(int sbiId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        var query = SqlQueries.CustomerPayment.GetSavedBankInformationById;
        
        var bankInfo = await connection.QuerySingleOrDefaultAsync<SavedBankInformationResponseDto>(
            new CommandDefinition(query, new { SBIID = sbiId }, cancellationToken: cancellationToken));
        
        return bankInfo;
    }

    public async Task<SavedBankInformationResponseDto> CreateSavedBankInformationAsync(CreateSavedBankInformationDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        try
        {
            var query = SqlQueries.CustomerPayment.CreateSavedBankInformation;
            
            var bankInfo = await connection.QuerySingleAsync<SavedBankInformationResponseDto>(
                new CommandDefinition(
                    query,
                    new
                    {
                        dto.CustomerID,
                        dto.BankName,
                        dto.AccountNo,
                        dto.IBAN
                    },
                    cancellationToken: cancellationToken));

            _logger.LogInformation("Created saved bank information {SBIID} for customer {CustomerID}", 
                bankInfo.SBIID, dto.CustomerID);
            
            return bankInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating saved bank information for customer {CustomerID}", dto.CustomerID);
            throw;
        }
    }

    public async Task<SavedBankInformationResponseDto> UpdateSavedBankInformationAsync(int sbiId, UpdateSavedBankInformationDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        try
        {
            var query = SqlQueries.CustomerPayment.UpdateSavedBankInformation;
            
            var bankInfo = await connection.QuerySingleAsync<SavedBankInformationResponseDto>(
                new CommandDefinition(
                    query,
                    new
                    {
                        SBIID = sbiId,
                        dto.BankName,
                        dto.AccountNo,
                        dto.IBAN
                    },
                    cancellationToken: cancellationToken));

            _logger.LogInformation("Updated saved bank information {SBIID}", sbiId);
            
            return bankInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating saved bank information {SBIID}", sbiId);
            throw;
        }
    }

    public async Task<bool> DeleteSavedBankInformationAsync(int sbiId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        try
        {
            var query = SqlQueries.CustomerPayment.DeleteSavedBankInformation;
            
            var result = await connection.ExecuteAsync(
                new CommandDefinition(query, new { SBIID = sbiId }, cancellationToken: cancellationToken));

            if (result > 0)
            {
                _logger.LogInformation("Deleted saved bank information {SBIID}", sbiId);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting saved bank information {SBIID}", sbiId);
            throw;
        }
    }
}
