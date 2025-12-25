using Dapper;
using BCrypt.Net;
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
    
    /// <summary>
    /// Get all customers
    /// </summary>
    public async Task<IReadOnlyList<CustomerResponseDto>> GetAllDtoAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        // Dapper maps SQL result columns to DTO properties by name (case-insensitive)
        var customers = await connection.QueryAsync<CustomerResponseDto>(
            // Build the query dynamically based on the parameters
            new CommandDefinition(SqlQueries.Customer.GetCustomerBase + " ORDER BY c.CustomerID", 
                cancellationToken: cancellationToken));
        
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
            
            // Build the query dynamically based on the parameters
            new CommandDefinition(SqlQueries.Customer.GetCustomerBase + " WHERE c.CustomerID = @CustomerID", 
                new { CustomerID = customerId }, 
                cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Get customer by email as DTO
    /// </summary>
    public async Task<CustomerResponseDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        // Dapper maps the SQL result to CustomerResponseDto
        return await connection.QueryFirstOrDefaultAsync<CustomerResponseDto>(
            // Build the query dynamically based on the parameters
            new CommandDefinition(SqlQueries.Customer.GetCustomerBase + " WHERE u.ContactEmail = @ContactEmail", 
                new { ContactEmail = email }, 
                cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    public async Task<CustomerResponseDto> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        int userId;
        
        using (var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken))
        {
            using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                // Hash password with BCrypt
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

                // Insert User (super-type) - create anonymous object from DTO + required UserType value
                userId = await connection.QuerySingleAsync<int>(
                    new CommandDefinition(SqlQueries.User.InsertUser, new
                    {
                        dto.UserName,
                        dto.ContactEmail,
                        dto.ContactPhone,
                        PasswordHash = passwordHash,
                        UserType = "Customer"
                    }, 
                        transaction, cancellationToken: cancellationToken));

                // Insert Customer (sub-type) - create anonymous object from DTO + generated values
                await connection.ExecuteAsync(
                    new CommandDefinition(SqlQueries.Customer.InsertCustomer, new
                    {
                        CustomerID = userId,
                        dto.CustomerType,
                        dto.ReliabilityStatus,
                        dto.City,
                        dto.Country
                    }, transaction, cancellationToken: cancellationToken));

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        // Retrieve the created customer using a new connection after transaction is committed and disposed
        return await GetByIdDtoAsync(userId, cancellationToken) 
            ?? throw new InvalidOperationException("Failed to retrieve created customer");
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

    /// <summary>
    /// Update only City and Country
    /// </summary>
    public async Task<CustomerResponseDto?> UpdateCustomerCityAndCountryAsync(UpdateCustomerCityAndCountryDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            // Update City if provided
            if (dto.City is not null && dto.City != string.Empty)
            {
                await connection.ExecuteAsync(
                    new CommandDefinition(SqlQueries.Customer.UpdateCustomerCity, new
                    {
                        CustomerID = dto.CustomerID,
                        dto.City
                    }, transaction, cancellationToken: cancellationToken));
            }

            // Update Country if provided
            if (dto.Country is not null && dto.Country != string.Empty)
            {
                await connection.ExecuteAsync(
                    new CommandDefinition(SqlQueries.Customer.UpdateCustomerCountry, new
                    {
                        CustomerID = dto.CustomerID,
                        dto.Country
                    }, transaction, cancellationToken: cancellationToken));
            }

            await transaction.CommitAsync(cancellationToken);
            return await GetByIdDtoAsync(dto.CustomerID, cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Delete a customer
    /// </summary>
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