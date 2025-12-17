using Dapper;
using Kismet.Core.Data;
using Kismet.Entities.DTOs;
using Kismet.Entities.Models;
using Kismet.Repository.Helpers;
using Kismet.Repository.Interfaces;

namespace Kismet.Repository.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    
    /// <summary>
    /// Get all users
    /// </summary>
    public async Task<IReadOnlyList<UserResponseDto>> GetAllDtoAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        // Dapper maps SQL result columns to DTO properties by name (case-insensitive)
        var users = await connection.QueryAsync<UserResponseDto>(
            new CommandDefinition(SqlQueries.User.GetAllDto, cancellationToken: cancellationToken));
        
        return users.ToList().AsReadOnly();
    }

    /// <summary>
    /// Get user by ID as DTO
    /// </summary>
    public async Task<UserResponseDto?> GetByIdDtoAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        // Dapper maps the SQL result to UserResponseDto  
        return await connection.QueryFirstOrDefaultAsync<UserResponseDto>(
            new CommandDefinition(SqlQueries.User.GetByIdDto, 
                new { UserID = userId }, 
                cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Update only UserName
    /// </summary>
    public async Task<UserResponseDto?> UpdateUserNameAsync(UpdateUserNameDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await connection.ExecuteAsync(
                new CommandDefinition(SqlQueries.User.UpdateUserName, new
                {
                    UserID = dto.UserID,
                    dto.UserName
                }, transaction, cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);

            // Return updated user as DTO
            return await GetByIdDtoAsync(dto.UserID, cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Update only Email
    /// </summary>
    public async Task<UserResponseDto?> UpdateUserEmailAsync(UpdateUserEmailDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await connection.ExecuteAsync(
                new CommandDefinition(SqlQueries.User.UpdateUserEmail, new
                {
                    UserID = dto.UserID,
                    dto.ContactEmail
                }, transaction, cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);
            return await GetByIdDtoAsync(dto.UserID, cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Update only Phone
    /// </summary>
    public async Task<UserResponseDto?> UpdateUserPhoneAsync(UpdateUserPhoneDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await connection.ExecuteAsync(
                new CommandDefinition(SqlQueries.User.UpdateUserPhone, new
                {
                    UserID = dto.UserID,
                    dto.ContactPhone
                }, transaction, cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);
            return await GetByIdDtoAsync(dto.UserID, cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Update only Password
    /// </summary>
    public async Task<UserResponseDto?> UpdateUserPasswordAsync(UpdateUserPasswordDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await connection.ExecuteAsync(
                new CommandDefinition(SqlQueries.User.UpdateUserPassword, new
                {
                    UserID = dto.UserID,
                    dto.PasswordHash
                }, transaction, cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);
            return await GetByIdDtoAsync(dto.UserID, cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int userId, CancellationToken cancellationToken = default)
    {
        // Delete from User
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {

            await connection.ExecuteAsync(
                new CommandDefinition(SqlQueries.User.DeleteUser, 
                    new { UserID = userId }, 
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

