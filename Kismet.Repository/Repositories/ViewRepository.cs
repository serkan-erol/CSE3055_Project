using Dapper;
using Kismet.Core.Data;
using Kismet.Entities.DTOs;
using Kismet.Repository.Helpers;
using Kismet.Repository.Interfaces;

namespace Kismet.Repository.Repositories;

public class ViewRepository : IViewRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ViewRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<OrderDetailsViewDto>> GetOrderDetailsByCustomerAsync(int customerId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var orders = await connection.QueryAsync<OrderDetailsViewDto>(
            new CommandDefinition(
                SqlQueries.Views.GetOrderDetailsByCustomer, 
                new { CustomerID = customerId }, 
                cancellationToken: cancellationToken));
        return orders.ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<ShipmentTrackingViewDto>> GetShipmentTrackingByCustomerAsync(int customerId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var shipments = await connection.QueryAsync<ShipmentTrackingViewDto>(
            new CommandDefinition(
                SqlQueries.Views.GetShipmentTrackingByCustomer, 
                new { CustomerID = customerId }, 
                cancellationToken: cancellationToken));
        return shipments.ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<FinancialOverviewViewDto>> GetFinancialOverviewByCustomerAsync(int customerId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var transactions = await connection.QueryAsync<FinancialOverviewViewDto>(
            new CommandDefinition(
                SqlQueries.Views.GetFinancialOverviewByCustomer, 
                new { CustomerID = customerId }, 
                cancellationToken: cancellationToken));
        return transactions.ToList().AsReadOnly();
    }

    public async Task<InventoryProductionStatusViewDto> GetInventoryStatusByFabricAsync(int fabricId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var inventory = await connection.QuerySingleOrDefaultAsync<InventoryProductionStatusViewDto>(
            new CommandDefinition(
                SqlQueries.Views.GetInventoryStatusByFabric, 
                new { FabricID = fabricId }, 
                cancellationToken: cancellationToken));
        return inventory ?? throw new InvalidOperationException("Fabric not found");
    }
}