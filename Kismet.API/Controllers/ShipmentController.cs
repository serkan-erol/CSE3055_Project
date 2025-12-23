using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Kismet.Entities.DTOs;
using Kismet.Repository.Interfaces;
using System.Security.Claims;

namespace Kismet.API.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ShipmentController : ControllerBase
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly ILogger<ShipmentController> _logger;

    public ShipmentController(
        IShipmentRepository shipmentRepository,
        ILogger<ShipmentController> logger)
    {
        _shipmentRepository = shipmentRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all shipments for an order (accessible by both customers and employees)
    /// </summary>
    /// <param name="orderId">The order ID</param>
    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetShipmentsByOrderId(int orderId, CancellationToken cancellationToken)
    {
        try
        {
            var shipments = await _shipmentRepository.GetShipmentsByOrderIdForEmployeeAsync(orderId, cancellationToken);
            return Ok(shipments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shipments for order {OrderId}", orderId);
            return StatusCode(500, "An error occurred while retrieving shipments");
        }
    }

    /// <summary>
    /// Get shipment details by ID (accessible by both customers and employees)
    /// </summary>
    /// <param name="shipmentId">The shipment ID</param>
    [HttpGet("{shipmentId}")]
    public async Task<IActionResult> GetShipmentById(int shipmentId, CancellationToken cancellationToken)
    {
        try
        {
            var shipment = await _shipmentRepository.GetShipmentByIdForEmployeeAsync(shipmentId, cancellationToken);
            return Ok(shipment);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Shipment {ShipmentId} not found", shipmentId);
            return NotFound(new { message = "Shipment not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shipment {ShipmentId}", shipmentId);
            return StatusCode(500, "An error occurred while retrieving shipment");
        }
    }

    /// <summary>
    /// Ship an order - creates shipments for all batches (employees only)
    /// </summary>
    /// <param name="orderId">The order ID to ship</param>
    ///[Authorize(Roles = EmployeeRole)]
    [HttpPost("ship/{orderId}")]
    public async Task<IActionResult> ShipOrder(int orderId, CancellationToken cancellationToken)
    {
        try
        {
            var existingBatches = await _shipmentRepository.GetBatchIdsByOrderIdAsync(orderId, cancellationToken);
            if (!existingBatches.Any())
            {
                return BadRequest(new { message = "No batches found for this order to ship." });
            }

            const int employeeId = 2;

            var dto = new ShipOrderDto
            {
                OrderID = orderId,
                EmployeeID = employeeId
            };

            var shipments = await _shipmentRepository.ShipOrderAsync(dto, cancellationToken);
            
            return Ok(new 
            { 
                message = $"Order shipped successfully. {shipments.Count} shipment(s) created.",
                shipments 
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Could not ship order {OrderId}", orderId);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error shipping order {OrderId}", orderId);
            return StatusCode(500, "An error occurred while shipping the order");
        }
    }

    /// <summary>
    /// Update expected delivery date (employees only)
    /// </summary>
    /// <param name="shipmentId">The shipment ID</param>
    /// <param name="dto">Update delivery date DTO</param>
    ///[Authorize(Roles = EmployeeRole)]
    [HttpPatch("{shipmentId}/delivery-date")]
    public async Task<IActionResult> UpdateExpectedDeliveryDate(
        int shipmentId, 
        [FromBody] UpdateExpectedDeliveryDateDto dto, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if shipment is locked
            var isLocked = await _shipmentRepository.CheckIfShipmentIsLockedAsync(shipmentId, cancellationToken);
            if (isLocked)
            {
                return BadRequest(new { message = "Cannot update delivery date. Shipment is locked." });
            }

            const int employeeId = 2;
            dto.ShipmentID = shipmentId;
            dto.EmployeeID = employeeId;

            var shipment = await _shipmentRepository.UpdateExpectedDeliveryDateAsync(dto, cancellationToken);
            return Ok(shipment);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Shipment {ShipmentId} not found", shipmentId);
            return NotFound(new { message = "Shipment not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating delivery date for shipment {ShipmentId}", shipmentId);
            return StatusCode(500, "An error occurred while updating delivery date");
        }
    }

    /// <summary>
    /// Update shipment status (employees only)
    /// </summary>
    /// <param name="shipmentId">The shipment ID</param>
    /// <param name="dto">Update status DTO</param>
    ///[Authorize(Roles = EmployeeRole)]
    [HttpPatch("{shipmentId}/status")]
    public async Task<IActionResult> UpdateShipmentStatus(
        int shipmentId, 
        [FromBody] UpdateShipmentStatusDto dto, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if shipment is locked
            var isLocked = await _shipmentRepository.CheckIfShipmentIsLockedAsync(shipmentId, cancellationToken);
            if (isLocked)
            {
                return BadRequest(new { message = "Cannot update status. Shipment is locked." });
            }

            dto.ShipmentID = shipmentId;

            var shipment = await _shipmentRepository.UpdateShipmentStatusAsync(dto, cancellationToken);
            return Ok(shipment);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Shipment {ShipmentId} not found", shipmentId);
            return NotFound(new { message = "Shipment not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for shipment {ShipmentId}", shipmentId);
            return StatusCode(500, "An error occurred while updating shipment status");
        }
    }

    /// <summary>
    /// Set actual delivery date (employees only)
    /// </summary>
    /// <param name="shipmentId">The shipment ID</param>
    /// <param name="dto">Set delivery date DTO</param>
    ///[Authorize(Roles = EmployeeRole)]
    [HttpPatch("{shipmentId}/actual-delivery")]
    public async Task<IActionResult> SetActualDeliveryDate(
        int shipmentId, 
        [FromBody] SetActualDeliveryDateDto dto, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if shipment is locked
            var isLocked = await _shipmentRepository.CheckIfShipmentIsLockedAsync(shipmentId, cancellationToken);
            if (isLocked)
            {
                return BadRequest(new { message = "Cannot set delivery date. Shipment is locked." });
            }

            dto.ShipmentID = shipmentId;

            var shipment = await _shipmentRepository.SetActualDeliveryDateAsync(dto, cancellationToken);
            
            // Auto-update status to Delivered when actual delivery date is set
            await _shipmentRepository.UpdateShipmentStatusAsync(
                new UpdateShipmentStatusDto 
                { 
                    ShipmentID = shipmentId, 
                    ShipmentStatus = Entities.Enums.ShipmentStatus.Delivered 
                }, 
                cancellationToken);

            return Ok(shipment);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Shipment {ShipmentId} not found", shipmentId);
            return NotFound(new { message = "Shipment not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting actual delivery date for shipment {ShipmentId}", shipmentId);
            return StatusCode(500, "An error occurred while setting actual delivery date");
        }
    }

    /// <summary>
    /// Update customs document reference (employees only)
    /// </summary>
    /// <param name="shipmentId">The shipment ID</param>
    /// <param name="dto">Update customs doc DTO</param>
    ///[Authorize(Roles = EmployeeRole)]
    [HttpPatch("{shipmentId}/customs")]
    public async Task<IActionResult> UpdateCustomsDocRef(
        int shipmentId, 
        [FromBody] UpdateCustomsDocRefDto dto, 
        CancellationToken cancellationToken)
    {
        try
        {
            dto.ShipmentID = shipmentId;

            var shipment = await _shipmentRepository.UpdateCustomsDocRefAsync(dto, cancellationToken);
            return Ok(shipment);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Shipment {ShipmentId} not found", shipmentId);
            return NotFound(new { message = "Shipment not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customs doc for shipment {ShipmentId}", shipmentId);
            return StatusCode(500, "An error occurred while updating customs document");
        }
    }

    /// <summary>
    /// Lock a shipment (employees only)
    /// </summary>
    /// <param name="shipmentId">The shipment ID</param>
    ///[Authorize(Roles = EmployeeRole)]
    [HttpPost("{shipmentId}/lock")]
    public async Task<IActionResult> LockShipment(int shipmentId, CancellationToken cancellationToken)
    {
        try
        {
            var dto = new LockShipmentDto { ShipmentID = shipmentId };
            var shipment = await _shipmentRepository.LockShipmentAsync(dto, cancellationToken);
            return Ok(new { message = "Shipment locked successfully", shipment });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Shipment {ShipmentId} not found", shipmentId);
            return NotFound(new { message = "Shipment not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error locking shipment {ShipmentId}", shipmentId);
            return StatusCode(500, "An error occurred while locking shipment");
        }
    }

    /// <summary>
    /// Unlock a shipment (employees only)
    /// </summary>
    /// <param name="shipmentId">The shipment ID</param>
    ///[Authorize(Roles = EmployeeRole)]
    [HttpPost("{shipmentId}/unlock")]
    public async Task<IActionResult> UnlockShipment(int shipmentId, CancellationToken cancellationToken)
    {
        try
        {
            var dto = new LockShipmentDto { ShipmentID = shipmentId };
            var shipment = await _shipmentRepository.UnlockShipmentAsync(dto, cancellationToken);
            return Ok(new { message = "Shipment unlocked successfully", shipment });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Shipment {ShipmentId} not found", shipmentId);
            return NotFound(new { message = "Shipment not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking shipment {ShipmentId}", shipmentId);
            return StatusCode(500, "An error occurred while unlocking shipment");
        }
    }

    /// <summary>
    /// Get shipment status display name (employees only)
    /// </summary>
    /// <param name="shipmentId">The shipment ID</param>
    ///[Authorize(Roles = EmployeeRole)]
    [HttpGet("{shipmentId}/status/display")]
    public async Task<IActionResult> GetShipmentStatusDisplayName(int shipmentId, CancellationToken cancellationToken)
    {
        try
        {
            var displayName = await _shipmentRepository.GetShipmentStatusDisplayNameByIdAsync(shipmentId, cancellationToken);
            return Ok(new { shipmentId, status = displayName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status display name for shipment {ShipmentId}", shipmentId);
            return StatusCode(500, "An error occurred while retrieving status");
        }
    }
}