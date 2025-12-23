using Dapper;
using Kismet.Core.Data;
using Kismet.Entities.DTOs;
using Kismet.Entities.Enums;
using Kismet.Repository.Helpers;
using Kismet.Repository.Interfaces;

namespace Kismet.Repository.Repositories;

public class ShipmentRepository : IShipmentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ShipmentRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Get all shipments for an order (customer view)
    /// </summary>
    public async Task<IReadOnlyList<ShipmentResponseToCustomerDto>> GetShipmentsByOrderIdForCustomerAsync(int orderId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var query = SqlQueries.Shipment.GetShipmentBaseForCustomer + " WHERE s.OrderID = @OrderID ORDER BY s.ShipmentID";

        var shipments = await connection.QueryAsync<ShipmentResponseToCustomerDto>(
            new CommandDefinition(query, new { OrderID = orderId }, cancellationToken: cancellationToken));
        return shipments.ToList().AsReadOnly();
    }

    /// <summary>
    /// Get all shipments for an order (employee view)
    /// </summary>
    public async Task<IReadOnlyList<ShipmentResponseToEmployeeDto>> GetShipmentsByOrderIdForEmployeeAsync(int orderId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var query = SqlQueries.Shipment.GetShipmentBaseForEmployee + " WHERE s.OrderID = @OrderID ORDER BY s.ShipmentID";

        var shipments = await connection.QueryAsync<ShipmentResponseToEmployeeDto>(
            new CommandDefinition(query, new { OrderID = orderId }, cancellationToken: cancellationToken));
        return shipments.ToList().AsReadOnly();
    }

    /// <summary>
    /// Get shipment details by ID (customer view)
    /// </summary>
    public async Task<ShipmentResponseToCustomerDto> GetShipmentByIdForCustomerAsync(int shipmentId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var query = SqlQueries.Shipment.GetShipmentBaseForCustomer + " WHERE s.ShipmentID = @ShipmentID";

        var shipment = await connection.QuerySingleOrDefaultAsync<ShipmentResponseToCustomerDto>(
            new CommandDefinition(query, new { ShipmentID = shipmentId }, cancellationToken: cancellationToken));
        return shipment ?? throw new InvalidOperationException("Shipment not found");
    }

    /// <summary>
    /// Get shipment details by ID (employee view)
    /// </summary>
    public async Task<ShipmentResponseToEmployeeDto> GetShipmentByIdForEmployeeAsync(int shipmentId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var query = SqlQueries.Shipment.GetShipmentBaseForEmployee + " WHERE s.ShipmentID = @ShipmentID";

        var shipment = await connection.QuerySingleOrDefaultAsync<ShipmentResponseToEmployeeDto>(
            new CommandDefinition(query, new { ShipmentID = shipmentId }, cancellationToken: cancellationToken));
        return shipment ?? throw new InvalidOperationException("Shipment not found");
    }

    /// <summary>
    /// Ship an order - creates shipments for all batches (max 50 batches per shipment)
    /// Calls stored procedure dbo.ShipOrder
    /// </summary>
    public async Task<IReadOnlyList<ShipOrderResponseDto>> ShipOrderAsync(ShipOrderDto dto, CancellationToken cancellationToken = default)
{
    using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

    var shipments = await connection.QueryAsync<ShipOrderResponseDto>(
        new CommandDefinition(
            SqlQueries.Shipment.ShipOrder,
            new { OrderID = dto.OrderID, EmployeeID = dto.EmployeeID },
            commandType: System.Data.CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

    return shipments.ToList().AsReadOnly();
}
    /// <summary>
    /// Update expected delivery date (employees only)
    /// </summary>
    public async Task<ShipmentResponseToEmployeeDto> UpdateExpectedDeliveryDateAsync(UpdateExpectedDeliveryDateDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                SqlQueries.Shipment.UpdateExpectedDeliveryDate, 
                dto, 
                cancellationToken: cancellationToken));

        return await GetShipmentByIdForEmployeeAsync(dto.ShipmentID, cancellationToken);
    }

    /// <summary>
    /// Update shipment status
    /// </summary>
    public async Task<ShipmentResponseToEmployeeDto> UpdateShipmentStatusAsync(UpdateShipmentStatusDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                SqlQueries.Shipment.UpdateShipmentStatus, 
                dto, 
                cancellationToken: cancellationToken));

        return await GetShipmentByIdForEmployeeAsync(dto.ShipmentID, cancellationToken);
    }

    /// <summary>
    /// Set actual delivery date
    /// </summary>
    public async Task<ShipmentResponseToEmployeeDto> SetActualDeliveryDateAsync(SetActualDeliveryDateDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                SqlQueries.Shipment.SetActualDeliveryDate, 
                dto, 
                cancellationToken: cancellationToken));

        return await GetShipmentByIdForEmployeeAsync(dto.ShipmentID, cancellationToken);
    }

    /// <summary>
    /// Update customs document reference
    /// </summary>
    public async Task<ShipmentResponseToEmployeeDto> UpdateCustomsDocRefAsync(UpdateCustomsDocRefDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                SqlQueries.Shipment.UpdateCustomsDocRef, 
                dto, 
                cancellationToken: cancellationToken));

        return await GetShipmentByIdForEmployeeAsync(dto.ShipmentID, cancellationToken);
    }

    /// <summary>
    /// Lock a shipment to prevent modifications
    /// </summary>
    public async Task<ShipmentResponseToEmployeeDto> LockShipmentAsync(LockShipmentDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                SqlQueries.Shipment.LockShipment, 
                dto, 
                cancellationToken: cancellationToken));

        return await GetShipmentByIdForEmployeeAsync(dto.ShipmentID, cancellationToken);
    }

    /// <summary>
    /// Check if a shipment is locked
    /// </summary>
    public async Task<bool> CheckIfShipmentIsLockedAsync(int shipmentId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var isLocked = await connection.QuerySingleOrDefaultAsync<bool>(
            new CommandDefinition(
                SqlQueries.Shipment.CheckIfShipmentIsLocked, 
                new { ShipmentID = shipmentId }, 
                cancellationToken: cancellationToken));

        return isLocked;
    }

    /// <summary>
    /// Get shipment status by ID
    /// </summary>
    public async Task<ShipmentStatus> GetShipmentStatusByIdAsync(int shipmentId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var status = await connection.QuerySingleOrDefaultAsync<ShipmentStatus>(
            new CommandDefinition(
                SqlQueries.Shipment.GetShipmentStatusById, 
                new { ShipmentID = shipmentId }, 
                cancellationToken: cancellationToken));

        return status;
    }

    /// <summary>
    /// Get the display name of a shipment status by ID
    /// </summary>
    public async Task<string> GetShipmentStatusDisplayNameByIdAsync(int shipmentId, CancellationToken cancellationToken = default)
    {
        var shipmentStatus = await GetShipmentStatusByIdAsync(shipmentId, cancellationToken);
        return shipmentStatus.GetDisplayName();
    }

    /// <summary>
    /// Get all batch IDs for an order
    /// </summary>
    public async Task<IReadOnlyList<int>> GetBatchIdsByOrderIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var query = SqlQueries.Shipment.GetBatchIdsByOrderId;

        var batchIds = await connection.QueryAsync<int>(
            new CommandDefinition(query, new { OrderID = orderId }, cancellationToken: cancellationToken));
        return batchIds.ToList().AsReadOnly();
    }
}