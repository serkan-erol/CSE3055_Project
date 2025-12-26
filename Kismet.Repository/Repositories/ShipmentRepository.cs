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
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;

    public ShipmentRepository(IDbConnectionFactory connectionFactory, 
                              IOrderRepository orderRepository, 
                              ICustomerRepository customerRepository)
    {
        _connectionFactory = connectionFactory;
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
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

        // Update shipment status to Delivered (2)
        await connection.ExecuteAsync(
            new CommandDefinition(
                SqlQueries.Shipment.UpdateShipmentStatus, 
                new { ShipmentID = dto.ShipmentID, ShipmentStatus = ShipmentStatus.Delivered }, 
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

    /// <summary>
    /// Update shipment origin and destination country
    /// </summary>
    public async Task<ShipmentResponseToEmployeeDto> UpdateShipmentCountriesAsync(UpdateShipmentCountriesDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        if (dto.OriginCountry is not null)
        {
            // Update shipment origin country
            await connection.ExecuteAsync(
            new CommandDefinition(
                SqlQueries.Shipment.UpdateShipmentOriginCountry, 
                dto, 
                cancellationToken: cancellationToken));
        }

        if (dto.DestinationCountry is not null)
        {
            // Update shipment destination country
            await connection.ExecuteAsync(
                new CommandDefinition(
                    SqlQueries.Shipment.UpdateShipmentDestinationCountry, 
                    dto, 
                    cancellationToken: cancellationToken));
        }

        // Return the updated shipment
        return await GetShipmentByIdForEmployeeAsync(dto.ShipmentID, cancellationToken);
    }
}