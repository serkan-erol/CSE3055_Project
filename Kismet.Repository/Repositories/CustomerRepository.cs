using Dapper;
using Kismet.Core.Data;
using Kismet.Entities.DTOs;
using Kismet.Entities.Models;
using Kismet.Repository.Helpers;
using Kismet.Repository.Interfaces;

namespace Kismet.Repository.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CustomerRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // Entity methods (for internal use if needed)
    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var customers = await connection.QueryAsync<Customer>(
            new CommandDefinition(SqlQueries.Customer.GetAll, cancellationToken: cancellationToken));
        return customers.ToList().AsReadOnly();
    }

    public async Task<Customer?> GetByIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QueryFirstOrDefaultAsync<Customer>(
            new CommandDefinition(SqlQueries.Customer.GetById, 
                new { CustomerID = customerId }, 
                cancellationToken: cancellationToken));
    }
    
    /// <summary>
    /// Get all customers
    /// </summary>
    public async Task<IReadOnlyList<CustomerResponseDto>> GetAllDtoAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        // Dapper maps SQL result columns to DTO properties by name (case-insensitive)
        var customers = await connection.QueryAsync<CustomerResponseDto>(
            new CommandDefinition(SqlQueries.Customer.GetAllDto, cancellationToken: cancellationToken));
        
        return customers.ToList().AsReadOnly();
    }

    /// <summary>
    /// Get customer by ID as DTO
    /// </summary>
    public async Task<CustomerResponseDto?> GetByIdDtoAsync(int customerId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        // Dapper maps the SQL result to CustomerResponseDto
        return await connection.QueryFirstOrDefaultAsync<CustomerResponseDto>(
            new CommandDefinition(SqlQueries.Customer.GetByIdDto, 
                new { CustomerID = customerId }, 
                cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Create a new customer from DTO - Dapper maps DTO properties to SQL parameters
    /// </summary>
    public async Task<CustomerResponseDto> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            // Insert User (super-type) - Dapper maps DTO properties to SQL parameters
            var userId = await connection.QuerySingleAsync<int>(
                new CommandDefinition(SqlQueries.User.InsertCustomerUser, dto, 
                    transaction, cancellationToken: cancellationToken));

            // Insert Customer (sub-type) - create anonymous object from DTO + generated values
            await connection.ExecuteAsync(
                new CommandDefinition(SqlQueries.Customer.InsertCustomer, new
                {
                    CustomerID = userId,
                    dto.CustomerType,
                    dto.ReliabilityStatus
                }, transaction, cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);

            // Return the created customer as DTO
            return await GetByIdDtoAsync(userId, cancellationToken) 
                ?? throw new InvalidOperationException("Failed to retrieve created customer");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Update only CustomerType
    /// </summary>
    public async Task<CustomerResponseDto?> UpdateCustomerTypeAsync(UpdateCustomerTypeDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await connection.ExecuteAsync(
                new CommandDefinition(SqlQueries.Customer.UpdateCustomerType, new
                {
                    CustomerID = dto.CustomerID,
                    dto.CustomerType
                }, transaction, cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);

            // Return updated customer as DTO
            return await GetByIdDtoAsync(dto.CustomerID, cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Update only ReliabilityStatus
    /// </summary>
    public async Task<CustomerResponseDto?> UpdateCustomerReliabilityAsync(UpdateCustomerReliabilityDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await connection.ExecuteAsync(
                new CommandDefinition(SqlQueries.Customer.UpdateCustomerReliability, new
                {
                    CustomerID = dto.CustomerID,
                    dto.ReliabilityStatus
                }, transaction, cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);
            return await GetByIdDtoAsync(dto.CustomerID, cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int customerId, CancellationToken cancellationToken = default)
    {
        // Delete from Customer first (sub-type), then User (super-type)
        // Note: This assumes CASCADE DELETE is not configured
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var customerDeleted = await connection.ExecuteAsync(
                new CommandDefinition(SqlQueries.Customer.DeleteCustomer, 
                    new { CustomerID = customerId }, 
                    transaction, cancellationToken: cancellationToken));

            if (customerDeleted == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }

            await connection.ExecuteAsync(
                new CommandDefinition(SqlQueries.User.DeleteUser, 
                    new { UserID = customerId }, 
                    transaction, cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}

