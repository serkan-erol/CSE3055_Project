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

    /// <summary>
    /// Create batches for multiple fabrics in an existing order
    /// Uses JSON to pass fabric items to SQL Server (SQL Server has built-in JSON support with OPENJSON)
    /// </summary>
    public async Task<CreateBatchesResponseDto> CreateBatchesForMultipleFabricsAsync(
        int orderId, 
        List<int> fabricIDs, 
        List<int> quantities, 
        List<string?>? qualityGrades, 
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        if (fabricIDs == null || quantities == null)
        {
            throw new ArgumentNullException("FabricIDs and Quantities cannot be null");
        }

        if (fabricIDs.Count != quantities.Count)
        {
            throw new ArgumentException("FabricIDs and Quantities must have the same count");
        }

        if (fabricIDs.Count == 0)
        {
            throw new ArgumentException("At least one fabric item must be provided");
        }

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        // Build JSON array for fabric items
        // JSON is used because SQL Server has excellent built-in JSON support (OPENJSON function)
        // which makes it easy to parse arrays of objects without needing table-valued parameters
        var fabricItems = new List<object>();
        for (int i = 0; i < fabricIDs.Count; i++)
        {
            fabricItems.Add(new
            {
                FabricID = fabricIDs[i],
                TotalFabricUnits = quantities[i],
                QualityGrade = (qualityGrades != null && i < qualityGrades.Count) 
                    ? qualityGrades[i] 
                    : (string?)null
            });
        }

        var fabricItemsJson = JsonSerializer.Serialize(fabricItems);

        // Execute stored procedure
        await connection.ExecuteAsync(
            new CommandDefinition(
                SqlQueries.Batch.CreateBatchesForMultipleFabrics,
                new
                {
                    OrderID = orderId,
                    FabricItemsJson = fabricItemsJson
                },
                commandType: System.Data.CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        var totalUnits = quantities.Sum();
        return new CreateBatchesResponseDto
        {
            Success = true,
            Message = $"Successfully created batches for Order {orderId} with {fabricIDs.Count} fabric(s) and {totalUnits} total fabric units"
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

    public async Task<decimal> CalculateOrderTotalAmountAsync(int orderId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var totalAmount = await connection.QuerySingleAsync<decimal>(
            new CommandDefinition(
                SqlQueries.Batch.CalculateOrderTotalAmount,
                new { OrderID = orderId },
                cancellationToken: cancellationToken));
        return totalAmount;
    }
}