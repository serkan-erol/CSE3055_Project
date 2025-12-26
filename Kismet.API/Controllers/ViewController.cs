using Microsoft.AspNetCore.Mvc;
using Kismet.Entities.DTOs;
using Kismet.Repository.Interfaces;

namespace Kismet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ViewsController : ControllerBase
{
    private readonly IViewRepository _viewRepository;
    private readonly ILogger<ViewsController> _logger;

    public ViewsController(IViewRepository viewRepository, ILogger<ViewsController> logger)
    {
        _viewRepository = viewRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive order details for a customer
    /// </summary>
    [HttpGet("orders/customer/{customerId}")]
    public async Task<IActionResult> GetOrderDetails(int customerId, CancellationToken cancellationToken)
    {
        try
        {
            var orders = await _viewRepository.GetOrderDetailsByCustomerAsync(customerId, cancellationToken);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order details for customer {CustomerId}", customerId);
            return StatusCode(500, "An error occurred while retrieving order details");
        }
    }

    /// <summary>
    /// Get shipment tracking information for a customer
    /// </summary>
    [HttpGet("shipments/customer/{customerId}")]
    public async Task<IActionResult> GetShipmentTracking(int customerId, CancellationToken cancellationToken)
    {
        try
        {
            var shipments = await _viewRepository.GetShipmentTrackingByCustomerAsync(customerId, cancellationToken);
            return Ok(shipments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shipment tracking for customer {CustomerId}", customerId);
            return StatusCode(500, "An error occurred while retrieving shipment tracking");
        }
    }

    /// <summary>
    /// Get financial overview for a customer
    /// </summary>
    [HttpGet("financial/customer/{customerId}")]
    public async Task<IActionResult> GetFinancialOverview(int customerId, CancellationToken cancellationToken)
    {
        try
        {
            var transactions = await _viewRepository.GetFinancialOverviewByCustomerAsync(customerId, cancellationToken);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting financial overview for customer {CustomerId}", customerId);
            return StatusCode(500, "An error occurred while retrieving financial overview");
        }
    }

    /// <summary>
    /// Get inventory and production status for a fabric
    /// </summary>
    [HttpGet("inventory/fabric/{fabricId}")]
    public async Task<IActionResult> GetInventoryStatus(int fabricId, CancellationToken cancellationToken)
    {
        try
        {
            var inventory = await _viewRepository.GetInventoryStatusByFabricAsync(fabricId, cancellationToken);
            return Ok(inventory);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Fabric {FabricId} not found", fabricId);
            return NotFound(new { message = "Fabric not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory status for fabric {FabricId}", fabricId);
            return StatusCode(500, "An error occurred while retrieving inventory status");
        }
    }
}