using Kismet.Entities.DTOs;

namespace Kismet.Repository.Interfaces;

public interface IViewRepository
{
    /// <summary>
    /// Get order details for a specific customer
    /// </summary>
    Task<IReadOnlyList<OrderDetailsViewDto>> GetOrderDetailsByCustomerAsync(int customerId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get shipment tracking for a specific customer
    /// </summary>
    Task<IReadOnlyList<ShipmentTrackingViewDto>> GetShipmentTrackingByCustomerAsync(int customerId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get financial overview for a specific customer
    /// </summary>
    Task<IReadOnlyList<FinancialOverviewViewDto>> GetFinancialOverviewByCustomerAsync(int customerId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get inventory/production status for a specific fabric
    /// </summary>
    Task<InventoryProductionStatusViewDto> GetInventoryStatusByFabricAsync(int fabricId, CancellationToken cancellationToken = default);
}