using Dapper;
using Kismet.Core.Data;
using Kismet.Entities.DTOs;
using Kismet.Entities.Models;
using Kismet.Repository.Helpers;
using Kismet.Repository.Interfaces;

namespace Kismet.Repository.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public EmployeeRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    
    /// <summary>
    /// Get all employees
    /// </summary>
    public async Task<IReadOnlyList<EmployeeResponseDto>> GetAllDtoAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        // Dapper maps SQL result columns to DTO properties by name (case-insensitive)
        var employees = await connection.QueryAsync<EmployeeResponseDto>(
            new CommandDefinition(SqlQueries.Employee.GetAllDto, cancellationToken: cancellationToken));
        
        return employees.ToList().AsReadOnly();
    }

    /// <summary>
    /// Get employee by ID as DTO
    /// </summary>
    public async Task<EmployeeResponseDto?> GetByIdDtoAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        // Dapper maps the SQL result to EmployeeResponseDto  
        return await connection.QueryFirstOrDefaultAsync<EmployeeResponseDto>(
            new CommandDefinition(SqlQueries.Employee.GetByIdDto, 
                new { EmployeeID = employeeId }, 
                cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Create a new employee from DTO - Dapper maps DTO properties to SQL parameters
    /// </summary>
    public async Task<EmployeeResponseDto> CreateAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            // Insert User (super-type) - Dapper maps DTO properties to SQL parameters
            var userId = await connection.QuerySingleAsync<int>(
                new CommandDefinition(SqlQueries.User.InsertEmployeeUser, dto, 
                    transaction, cancellationToken: cancellationToken));

            // Insert Employee (sub-type) - create anonymous object from DTO + generated values
            await connection.ExecuteAsync(
                new CommandDefinition(SqlQueries.Employee.InsertEmployee, new
                {
                    EmployeeID = userId,
                    dto.EmployeeRole,
                    dto.AccessLevel
                }, transaction, cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);

            // Return the created employee as DTO
            return await GetByIdDtoAsync(userId, cancellationToken) 
                ?? throw new InvalidOperationException("Failed to retrieve created employee");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Update only EmployeeRole
    /// </summary>
    public async Task<EmployeeResponseDto?> UpdateEmployeeRoleAsync(UpdateEmployeeRoleDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await connection.ExecuteAsync(
                new CommandDefinition(SqlQueries.Employee.UpdateEmployeeRole, new
                {
                    EmployeeID = dto.EmployeeID,
                    dto.EmployeeRole
                }, transaction, cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);

            // Return updated employee as DTO
            return await GetByIdDtoAsync(dto.EmployeeID, cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Update only AccessLevel
    /// </summary>
    public async Task<EmployeeResponseDto?> UpdateEmployeeAccessLevelAsync(UpdateEmployeeAccessLevelDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await connection.ExecuteAsync(
                new CommandDefinition(SqlQueries.Employee.UpdateEmployeeAccessLevel, new
                {
                    EmployeeID = dto.EmployeeID,
                    dto.AccessLevel
                }, transaction, cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);
            return await GetByIdDtoAsync(dto.EmployeeID, cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        // Delete from Employee first (sub-type), then User (super-type)
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var employeeDeleted = await connection.ExecuteAsync(
                new CommandDefinition(SqlQueries.Employee.DeleteEmployee, 
                    new { EmployeeID = employeeId }, 
                    transaction, cancellationToken: cancellationToken));

            if (employeeDeleted == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }

            await connection.ExecuteAsync(
                new CommandDefinition(SqlQueries.User.DeleteUser, 
                    new { UserID = employeeId }, 
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