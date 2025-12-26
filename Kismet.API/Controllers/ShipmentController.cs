using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Kismet.Entities.DTOs;
using Kismet.Entities.Enums;
using Kismet.Repository.Interfaces;
using System.Security.Claims;

namespace Kismet.API.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ShipmentController : ControllerBase
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IEmployeeRepository _employeeRepository;

    // Constants for error messages
    private const string ShipmentNotFoundMessage = "Shipment not found";
    private const string ShipmentNotFoundLogMessage = "Shipment {ShipmentId} not found";
    private const string ErrorGettingShipmentsMessage = "An error occurred while retrieving shipments";
    private const string ErrorGettingShipmentMessage = "An error occurred while retrieving shipment";
    private const string ErrorShippingOrderMessage = "An error occurred while shipping the order";
    private const string ErrorUpdatingDeliveryDateMessage = "An error occurred while updating delivery date";
    private const string ErrorUpdatingShipmentStatusMessage = "An error occurred while updating shipment status";
    private const string ErrorSettingActualDeliveryMessage = "An error occurred while setting actual delivery date";
    private const string ErrorUpdatingCustomsDocMessage = "An error occurred while updating customs document";
    private const string ErrorLockingShipmentMessage = "An error occurred while locking shipment";
    private const string ErrorRetrievingStatusMessage = "An error occurred while retrieving status";
    private const string ShipmentLockedMessage = "Cannot update delivery date. Shipment is locked.";
    private const string ShipmentLockedStatusMessage = "Cannot update status. Shipment is locked.";
    private const string ShipmentLockedActualDeliveryMessage = "Cannot set delivery date. Shipment is locked.";
    private const int minAccessLevel = 3;

    public ShipmentController(
        IShipmentRepository shipmentRepository,
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IEmployeeRepository employeeRepository)
    {
        _shipmentRepository = shipmentRepository;
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _employeeRepository = employeeRepository;
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
            return StatusCode(500, new { error = ex.Message });
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
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Ship an order - creates shipments for all batches (employees only)
    /// </summary>
    /// <param name="employeeId">The employee ID from the route</param>
    /// <param name="orderId">The order ID to ship, from the route</param>
    ///[Authorize(Roles = EmployeeRole)]
    [HttpPost("ship/{employeeId:int}/{orderId:int}")]
    public async Task<IActionResult> ShipOrder(int employeeId, int orderId, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the employee exists
            var employee = await _employeeRepository.GetByIdDtoAsync(employeeId, cancellationToken);
            if (employee is null)
            {
                return NotFound(new { error = "Employee not found." });
            }

            // Check if the employee's access level is at least 3
            if (employee.AccessLevel < minAccessLevel)
            {
                return BadRequest(new { error = "Employee does not have permission to ship orders." });
            }

            var existingBatches = await _shipmentRepository.GetBatchIdsByOrderIdAsync(orderId, cancellationToken);
            if (!existingBatches.Any())
            {
                return BadRequest(new { error = "No batches found for this order to ship." });
            }

            // Check if the order exists
            var order = await _orderRepository.GetOrderByIdForEmployeeAsync(orderId, cancellationToken);
            if (order is null)
            {
                return NotFound(new { error = "Order not found." });
            }

            // Check if the order is approved
            if (order.OrderStatus != OrderStatus.Approved)
            {
                return BadRequest(new { error = "Order is not approved. It cannot be shipped." });
            }

            var dto = new ShipOrderDto
            {
                EmployeeID = employeeId,
                OrderID = orderId  
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
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update expected delivery date (employees only)
    /// </summary>
    /// <param name="shipmentId">The shipment ID</param>
    /// <param name="dto">Update delivery date DTO</param>
    ///[Authorize(Roles = EmployeeRole)]
    [HttpPatch("{employeeId:int}/{shipmentId}/delivery-date")]
    public async Task<IActionResult> UpdateExpectedDeliveryDate(
        int employeeId, int shipmentId, 
        [FromBody] UpdateExpectedDeliveryDateDto dto, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if the employee exists
            var employee = await _employeeRepository.GetByIdDtoAsync(employeeId, cancellationToken);
            if (employee is null)
            {
                return NotFound(new { error = "Employee not found." });
            }

            // Check if the employee's access level is at least 3
            if (employee.AccessLevel < minAccessLevel)
            {
                return BadRequest(new { error = "Employee does not have permission to update expected delivery date." });
            }

            // Check if shipment is locked
            var isLocked = await _shipmentRepository.CheckIfShipmentIsLockedAsync(shipmentId, cancellationToken);
            if (isLocked)
            {
                return BadRequest(new { error = ShipmentLockedMessage });
            }

            dto.ShipmentID = shipmentId;
            dto.EmployeeID = employeeId;

            var shipment = await _shipmentRepository.UpdateExpectedDeliveryDateAsync(dto, cancellationToken);
            return Ok(shipment);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update shipment status (employees only)
    /// </summary>
    /// <param name="employeeId">The employee ID</param>
    /// <param name="shipmentId">The shipment ID</param>
    /// <param name="dto">Update status DTO</param>
    ///[Authorize(Roles = EmployeeRole)]
    [HttpPatch("{employeeId:int}/{shipmentId}/status")]
    public async Task<IActionResult> UpdateShipmentStatus(
        int employeeId, int shipmentId, 
        [FromBody] UpdateShipmentStatusDto dto, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if the employee exists
            var employee = await _employeeRepository.GetByIdDtoAsync(employeeId, cancellationToken);
            if (employee is null)
            {
                return NotFound(new { error = "Employee not found." });
            }

            // Check if the employee's access level is at least 3
            if (employee.AccessLevel < minAccessLevel)
            {
                return BadRequest(new { error = "Employee does not have permission to update shipment status." });
            }

            // Check if shipment is locked
            var isLocked = await _shipmentRepository.CheckIfShipmentIsLockedAsync(shipmentId, cancellationToken);
            if (isLocked)
            {
                return BadRequest(new { error = ShipmentLockedStatusMessage });
            }

            dto.ShipmentID = shipmentId;

            var shipment = await _shipmentRepository.UpdateShipmentStatusAsync(dto, cancellationToken);
            return Ok(shipment);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Set actual delivery date (employees only)
    /// This method will also update the shipment status to Delivered (2)
    /// </summary>
    /// <param name="employeeId">The employee ID</param>
    /// <param name="shipmentId">The shipment ID</param>
    /// <param name="dto">Set delivery date DTO</param>
    ///[Authorize(Roles = EmployeeRole)]
    [HttpPatch("{employeeId:int}/{shipmentId}/actual-delivery")]
    public async Task<IActionResult> SetActualDeliveryDate(
        int employeeId, int shipmentId, 
        [FromBody] SetActualDeliveryDateDto dto, 
        CancellationToken cancellationToken)
    {
        try
        {

            // Check if the employee exists
            var employee = await _employeeRepository.GetByIdDtoAsync(employeeId, cancellationToken);
            if (employee is null)
            {
                return NotFound(new { error = "Employee not found." });
            }

            // Check if the employee's access level is at least 3
            if (employee.AccessLevel < minAccessLevel)
            {
                return BadRequest(new { error = "Employee does not have permission to set actual delivery date." });
            }

            // Check if shipment is locked
            var isLocked = await _shipmentRepository.CheckIfShipmentIsLockedAsync(shipmentId, cancellationToken);
            if (isLocked)
            {
                return BadRequest(new { error = ShipmentLockedActualDeliveryMessage });
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
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update customs document reference (employees only)
    /// </summary>
    /// <param name="employeeId">The employee ID</param>
    /// <param name="shipmentId">The shipment ID</param>
    /// <param name="dto">Update customs doc DTO</param>
    ///[Authorize(Roles = EmployeeRole)]
    [HttpPatch("{employeeId:int}/{shipmentId}/customs")]
    public async Task<IActionResult> UpdateCustomsDocRef(
        int employeeId, int shipmentId, 
        [FromBody] UpdateCustomsDocRefDto dto, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if the employee exists
            var employee = await _employeeRepository.GetByIdDtoAsync(employeeId, cancellationToken);
            if (employee is null)
            {
                return NotFound(new { error = "Employee not found." });
            }

            // Check if the employee's access level is at least 3
            if (employee.AccessLevel < minAccessLevel)
            {
                return BadRequest(new { error = "Employee does not have permission to update customs document reference." });
            }

            dto.ShipmentID = shipmentId;

            var shipment = await _shipmentRepository.UpdateCustomsDocRefAsync(dto, cancellationToken);
            return Ok(shipment);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
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
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update shipment origin and destination country (employees only)
    /// </summary>
    /// <param name="employeeId">The employee ID</param>
    /// <param name="shipmentId">The shipment ID</param>
    /// <param name="dto">Update countries DTO</param>
    ///[Authorize(Roles = EmployeeRole)]
    [HttpPatch("{employeeId:int}/{shipmentId}/countries")]
    public async Task<IActionResult> UpdateShipmentCountries(int employeeId, int shipmentId, [FromBody] UpdateShipmentCountriesDto dto, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the employee exists
            var employee = await _employeeRepository.GetByIdDtoAsync(employeeId, cancellationToken);
            if (employee is null)
            {
                return NotFound(new { error = "Employee not found." });
            }

            // Check if the employee's access level is at least 3
            if (employee.AccessLevel < minAccessLevel)
            {
                return BadRequest(new { error = "Employee does not have permission to update shipment origin and destination country." });
            }

            var shipment = await _shipmentRepository.GetShipmentByIdForEmployeeAsync(shipmentId, cancellationToken);
            
            // Check if the Shipment exists
            if(shipment is null)
            {
                return NotFound(new { error = "Shipment not found" });
            }
            
            dto.ShipmentID = shipmentId;

            // Check if the Order exists
            var order = await _orderRepository.GetOrderByIdForEmployeeAsync(shipment.OrderID, cancellationToken);
            if(order is null)
            {
                return NotFound(new { error = "Order not found" });
            }

            dto.OrderType = order.OrderType;

            // Check if the Customer exists
            var customer = await _customerRepository.GetByIdDtoAsync(order.CustomerID, cancellationToken);
            if(customer is null)
            {
                return NotFound(new { error = "Customer not found" });
            }

            if (order.OrderType == "Purchase")
            {
                dto.OriginCountry = "Türkiye";
                dto.DestinationCountry = customer.Country;
            }
            else
            {
                dto.OriginCountry = customer.Country;
                dto.DestinationCountry = "Türkiye";
            }

            // Update the shipment origin and destination country
            var updatedShipment = await _shipmentRepository.UpdateShipmentCountriesAsync(dto, cancellationToken);
            return Ok(updatedShipment);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}