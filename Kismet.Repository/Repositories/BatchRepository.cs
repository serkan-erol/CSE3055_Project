using Dapper;
using Kismet.Core.Data;
using Kismet.Entities.DTOs;
using Kismet.Repository.Helpers;
using Kismet.Repository.Interfaces;
using System.Text.Json;

namespace Kismet.Repository.Repositories;

public class BatchRepository : IBatchRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BatchRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<BatchResponseToCustomerDto>> GetBatchesByOrderIdForCustomerAsync(int orderId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var batches = await connection.QueryAsync<BatchResponseToCustomerDto>(
            new CommandDefinition(
                SqlQueries.Batch.GetBatchesForCustomer, 
                new { OrderID = orderId }, 
                cancellationToken: cancellationToken));
        return batches.ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<BatchResponseToEmployeeDto>> GetBatchesByOrderIdForEmployeeAsync(int orderId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var batches = await connection.QueryAsync<BatchResponseToEmployeeDto>(
            new CommandDefinition(
                SqlQueries.Batch.GetBatchesForEmployee, 
                new { OrderID = orderId }, 
                cancellationToken: cancellationToken));
        return batches.ToList().AsReadOnly();
    }

    public async Task<BatchResponseToEmployeeDto> GetBatchByIdAsync(int batchId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var batch = await connection.QuerySingleOrDefaultAsync<BatchResponseToEmployeeDto>(
            new CommandDefinition(
                SqlQueries.Batch.GetBatchById, 
                new { BatchID = batchId }, 
                cancellationToken: cancellationToken));
        return batch ?? throw new InvalidOperationException("Batch not found");
    }

   public async Task<CreateBatchesResponseDto> CreateBatchesAsync(CreateBatchesDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        // Execute stored procedure
        await connection.ExecuteAsync(
            new CommandDefinition(
                SqlQueries.Batch.CreateBatches,
                new
                {
                    OrderID = dto.OrderID,
                    FabricID = dto.FabricID,
                    TotalFabricUnits = dto.TotalFabricUnits,
                    QualityGrade = dto.QualityGrade
                },
                commandType: System.Data.CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return new CreateBatchesResponseDto
        {
            Success = true,
            Message = $"Successfully created batches for Order {dto.OrderID} with {dto.TotalFabricUnits} total fabric units"
        };
    }

    public async Task<IReadOnlyList<BatchResponseToEmployeeDto>> GetUnshippedBatchesByOrderIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var batches = await connection.QueryAsync<BatchResponseToEmployeeDto>(
            new CommandDefinition(
                SqlQueries.Batch.GetUnshippedBatches, 
                new { OrderID = orderId }, 
                cancellationToken: cancellationToken));
        return batches.ToList().AsReadOnly();
    }
}