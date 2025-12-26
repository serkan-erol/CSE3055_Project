using Kismet.Entities.DTOs;
using Kismet.Entities.Enums;
using Kismet.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kismet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IFinancialTransactionRepository _financialTransactionRepository;
    private readonly IBillingRepository _billingRepository;
    private readonly IBatchRepository _batchRepository;
    private readonly IShipmentRepository _shipmentRepository;

    private const int minAccessLevel = 3;

    public OrderController(IOrderRepository orderRepository, 
                           ICustomerRepository customerRepository,
                           IEmployeeRepository employeeRepository,
                           IFinancialTransactionRepository financialTransactionRepository,
                           IBillingRepository billingRepository,
                           IBatchRepository batchRepository,
                           IShipmentRepository shipmentRepository)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _employeeRepository = employeeRepository;
        _financialTransactionRepository = financialTransactionRepository;
        _billingRepository = billingRepository;
        _batchRepository = batchRepository;
        _shipmentRepository = shipmentRepository;
    }

    /// <summary>
    /// Get all orders of a customer to be seen by the customer
    /// </summary>
    [HttpGet("{customerId:int}/all-orders/for-customers")]
    public async Task<IActionResult> GetAllForCustomerAsync(int customerId, CancellationToken cancellationToken)
    {
        try
        {
            // Check if customer exists
            var customer = await _customerRepository.GetByIdDtoAsync(customerId, cancellationToken);
            if (customer is null)
            {
                return NotFound(new { error = "Customer not found" });
            }

            // Get all orders for the customer
            var orders = await _orderRepository.GetOrderForCustomerAsync(customerId, cancellationToken);
            
            // Check if no orders found
            //if (orders.Count == 0)
            //{
            //    return NotFound(new { error = "No orders found" });
            //}

            // Return the orders
            return Ok(orders);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get order by ID for a customer to be seen by the customer
    /// </summary>
    [HttpGet("{customerId:int}/{orderId:int}/order-by-id/for-customers")]
    public async Task<IActionResult> GetOrderByIdForCustomerAsync(int customerId, int orderId, CancellationToken cancellationToken)
    {
        try
        {
            // Check if customer exists
            var customer = await _customerRepository.GetByIdDtoAsync(customerId, cancellationToken);
            if (customer is null)
            {
                return NotFound(new { error = "Customer not found" });
            }
            
            // Get the order by ID for the customer
            var order = await _orderRepository.GetOrderByIdForCustomerAsync(customerId, orderId, cancellationToken);
            
            // Check if order not found
            if (order is null)
            {
                return NotFound(new { error = "Order not found" });
            }

            // Return the order
            return Ok(order);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get order by Order Number for a customer to see
    /// </summary>
    [HttpGet("{orderNumber}/order-by-order-number/for-customers")]
    public async Task<IActionResult> GetOrderByOrderNumberForCustomerAsync(string orderNumber, CancellationToken cancellationToken)
    {
        try
        {
            // Get the order by Order Number for the customer
            var order = await _orderRepository.GetOrderByOrderNumberForCustomerAsync(orderNumber, cancellationToken);
            
            // Check if order not found
            if (order is null)
            {
                return NotFound(new { error = "Order not found" });
            }
            return Ok(order);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Get order by ID for employees to see
    /// </summary>
    [HttpGet("{orderId:int}/order-by-id/for-employees")]
    public async Task<IActionResult> GetOrderByIdForEmployeeAsync(int orderId, CancellationToken cancellationToken)
    {
        try
        {
            // Get the order by ID
            var order = await _orderRepository.GetOrderByIdForEmployeeAsync(orderId, cancellationToken);
            
            // Check if order not found
            if (order is null)
            {
                return NotFound(new { error = "Order not found" });
            }

            // Return the order
            return Ok(order);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get order by Customer ID for employees to see
    /// </summary>
    [HttpGet("{customerId:int}/order-by-customer-id/for-employees")]
    public async Task<IActionResult> GetOrderByCustomerIdForEmployeeAsync(int customerId, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the customer exists
            var customer = await _customerRepository.GetByIdDtoAsync(customerId, cancellationToken);
            if (customer is null)
            {
                return NotFound(new { error = "Customer not found" });
            }

            // Get the order by Customer ID
            var orders = await _orderRepository.GetOrderByCustomerIdForEmployeeAsync(customerId, cancellationToken);
            
            // Check if no orders found
            if (orders.Count == 0)
            {
                return NotFound(new { error = "No orders found" });
            }

            return Ok(orders);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get order by Order Number for employees to see
    /// </summary>
    [HttpGet("{orderNumber}/order-by-order-number/for-employees")]
    public async Task<IActionResult> GetOrderByOrderNumberForEmployeeAsync(string orderNumber, CancellationToken cancellationToken)
    {
        try
        {
            // Get the order by Order Number
            var order = await _orderRepository.GetOrderByOrderNumberForEmployeeAsync(orderNumber, cancellationToken);
            
            // Check if order not found
            if (order is null)
            {
                return NotFound(new { error = "Order not found" });
            }

            // Return the order
            return Ok(order);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get order by Customer Number for employees to see
    /// </summary>
    [HttpGet("{customerNumber}/order-by-customer-number/for-employees")]
    public async Task<IActionResult> GetOrderByCustomerNumberForEmployeeAsync(string customerNumber, CancellationToken cancellationToken)
    {
        try
        {
            // Get the order by Customer Number
            var orders = await _orderRepository.GetOrderByCustomerNumberForEmployeeAsync(customerNumber, cancellationToken);
            
            // Check if no orders found
            if (orders.Count == 0)
            {
                return NotFound(new { error = "No orders found" });
            }
            return Ok(orders);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new order for a customer
    /// </summary>
    [HttpPost("{customerId:int}/create-order")]
    public async Task<IActionResult> CreateOrderAsync(int customerId, [FromBody] CreateOrderDto dto, CancellationToken cancellationToken)
    {
        try
        {
            // Check if model state is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the customer exists before creating the order
            var customer = await _customerRepository.GetByIdDtoAsync(customerId, cancellationToken);
            if (customer is null)
            {
                return NotFound(new { error = "Customer not found" });
            }

            // Set the customer ID
            dto.CustomerID = customerId;

            // Validate that FabricIDs and Quantities are provided and match
            if (dto.FabricIDs == null || dto.Quantities == null || dto.FabricIDs.Count == 0 || dto.Quantities.Count == 0)
            {
                return BadRequest(new { error = "FabricIDs and Quantities must be provided and cannot be empty" });
            }

            if (dto.FabricIDs.Count != dto.Quantities.Count)
            {
                return BadRequest(new { error = "FabricIDs and Quantities must have the same count" });
            }

            // Create the order first (with TotalAmount = 0)
            var order = await _orderRepository.CreateOrderAsync(dto, cancellationToken);

            // Create batches for all fabrics
            await _batchRepository.CreateBatchesForMultipleFabricsAsync(
                order.OrderID, 
                dto.FabricIDs, 
                dto.Quantities, 
                dto.QualityGrades, 
                cancellationToken);

            // Get the updated order to retrieve the calculated TotalAmount
            // A trigger has updated it based on batch prices
            var updatedOrder = await _orderRepository.GetOrderByIdForCustomerAsync(customerId, order.OrderID, cancellationToken);
            
            // Create the associated financial transaction
            var financialTransactionDto = new CreateFTDto
            {
                CustomerID = customerId,
                BillingID = 0,
                OrderID = order.OrderID,
                TransactionType = dto.OrderType,
                TotalAmount = updatedOrder.TotalAmount, // Use the calculated TotalAmount from the order
                TransactionDate = DateTime.UtcNow.Date.AddDays(-1),
            };

            // Find or create a suitable BillingID for the FinancialTransaction
            // Check if there are any suitable Billing entries for the FT
            var suitableBillingEntry = await _financialTransactionRepository.FindSuitableBillingEntryForFTAsync(financialTransactionDto, cancellationToken);
            
            // If there is not any, create a new Billing entry for the FT
            if (suitableBillingEntry is null)
            {

                // Create a new Billing entry for the FT
                var newBillingDto = new CreateBillingDto
                {
                    CustomerID = customerId,
                    BillingType = financialTransactionDto.TransactionType,
                    InvoiceNumber = Guid.NewGuid().ToString(), // Generate a unique invoice number
                    TotalDue = financialTransactionDto.TotalAmount,
                    BillingDate = financialTransactionDto.TransactionDate,
                };

                // Create the new Billing entry and set the BillingID in the DTO
                var newBilling = await _billingRepository.CreateBillingAsync(newBillingDto, cancellationToken);
                financialTransactionDto.BillingID = newBilling.BillingID;
            }
            else
            {
                // If the given BillingID is suitable, set the BillingID in the DTO
                financialTransactionDto.BillingID = suitableBillingEntry.BillingID;
                var updateBillingDto = new UpdateBillingDto
                {
                    BillingID = suitableBillingEntry.BillingID,
                    TotalDue = updatedOrder.TotalAmount,
                };
                await _billingRepository.UpdateBillingAsync(customerId, suitableBillingEntry.BillingID, updateBillingDto, cancellationToken);
            }

            // Create the financial transaction
            await _financialTransactionRepository.CreateFTAsync(financialTransactionDto, cancellationToken);

            // Return the updated order (with calculated TotalAmount)
            return Ok(updatedOrder);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Status update for an order by an employee
    /// </summary>
    [HttpPut("{employeeId:int}/update-order-status/")]
    public async Task<IActionResult> UpdateOrderStatusAsync(int employeeId, [FromBody] UpdateOrderStatusDto dto, CancellationToken cancellationToken)
    {
        try
        {
            // Check if model state is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the employee exists
            var employee = await _employeeRepository.GetByIdDtoAsync(employeeId, cancellationToken);
            if (employee is null)
            {
                return NotFound(new { error = "Employee not found" });
            }

            // Check if the employee's access level is at least 3
            if (employee.AccessLevel < minAccessLevel)
            {
                return BadRequest(new { error = "Employee does not have permission to update order status." });
            }

            // Check if the order exists
            var order = await _orderRepository.GetOrderByIdForEmployeeAsync(dto.OrderID, cancellationToken);
            if (order is null)
            {
                return NotFound(new { error = "Order not found" });
            }

            // Check if the order is currently locked
            // Redundant check, since the DB trigger will prevent updates if the order is locked
            //aaa var isLocked = await _orderRepository.CheckIfOrderIsLockedAsync(dto.OrderID, cancellationToken);
            // if (isLocked)
            // {
            //    return BadRequest(new { error = "Order is locked. It cannot be updated." });
            // }

            // OrderStatus checks to see if it is valid

            // This should never happen, this is a fallback. We must use the approve-order endpoint to approve an order.
            // Check if the new order status is approved
            if (dto.OrderStatus == OrderStatus.Approved)
            {
                return BadRequest(new { error = "Use approve-order endpoint to approve an order." });
            }

            // Check if the order is approved before updating the order status
            if (!order.IsApproved)
            {
                return BadRequest(new { error = "Order is not approved. It cannot be updated." });
            }
            
            // This should never happen, this is a fallback
            // Check if the order is already completed but if we are here, IsLocked is false!!!
            // Redundant check, since the DB trigger will prevent updates if the order is completed
            //aaa if (order.OrderStatus >= OrderStatus.Delivered)
            // {
            //    return BadRequest(new { error = "Order is already completed. It cannot be updated. Check the order's IsLocked status!" });
            // }

            // Check if the new order status is the same or less than the current order status
            if (dto.OrderStatus <= order.OrderStatus)
            {
                return BadRequest(new { error = "New order status is the same or less than the current order status." });
            }

            // Update the order status
            var updatedOrder = await _orderRepository.UpdateOrderStatusAsync(dto, cancellationToken);

            // Return the order
            return Ok(updatedOrder);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Cancel an order by an employee
    /// </summary>
    [HttpPut("{employeeId:int}/cancel-order/")]
    public async Task<IActionResult> CancelOrderAsync(int employeeId, int orderId, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the employee exists
            var employee = await _employeeRepository.GetByIdDtoAsync(employeeId, cancellationToken);
            if (employee is null)
            {
                return NotFound(new { error = "Employee not found" });
            }

            // Check if the employee's access level is at least 3
            if (employee.AccessLevel < minAccessLevel)
            {
                return BadRequest(new { error = "Employee does not have permission to cancel orders." });
            }

            // Check if the order exists
            var order = await _orderRepository.GetOrderByIdForEmployeeAsync(orderId, cancellationToken);
            if (order is null)
            {
                return NotFound(new { error = "Order not found" });
            }

            // Check if the order is already cancelled
            if (order.OrderStatus == OrderStatus.Cancelled)
            {
                return BadRequest(new { error = "Order is already cancelled. It cannot be cancelled again." });
            }
            
            // Update the order status to cancelled
            var updatedOrder = await _orderRepository.UpdateOrderStatusAsync(
                new UpdateOrderStatusDto 
                { 
                    OrderID = orderId, 
                    OrderStatus = OrderStatus.Cancelled,
                }, cancellationToken);

            // Return the updated order
            return Ok(updatedOrder);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Approve an order
    /// </summary>
    [HttpPut("{employeeId:int}/approve-order/")]
    public async Task<IActionResult> ApproveOrderAsync(int employeeId, [FromBody] ApproveOrderDto dto, CancellationToken cancellationToken)
    {
        try
        {
            // Check if model state is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the employee exists
            var employee = await _employeeRepository.GetByIdDtoAsync(employeeId, cancellationToken);
            if (employee is null)
            {
                return NotFound(new { error = "Employee not found" });
            }

            // Check if the employee's access level is at least 3
            if (employee.AccessLevel < minAccessLevel)
            {
                return BadRequest(new { error = "Employee does not have permission to approve orders." });
            }

            // Check if the order exists
            var order = await _orderRepository.GetOrderByIdForEmployeeAsync(dto.OrderID, cancellationToken);
            if (order is null)
            {
                return NotFound(new { error = "Order not found" });
            }

            // Check if the order status is pending
            var orderStatus = await _orderRepository.GetOrderStatusByIdAsync(dto.OrderID, cancellationToken);
            if (orderStatus != OrderStatus.Pending)
            {
                return BadRequest(new { error = "Order is not pending. It cannot be approved." });
            }

            // Set the ApprovedBy to the EmployeeID
            dto.ApprovedBy = employeeId;

            // Approve the order
            var approvedOrder = await _orderRepository.ApproveOrderAsync(dto, cancellationToken);

            //Create the Shipments for the order
            await _shipmentRepository.ShipOrderAsync(new ShipOrderDto { OrderID = approvedOrder.OrderID, EmployeeID = employeeId }, cancellationToken);

            // Return the approved order
            return Ok(approvedOrder);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

        /// <summary>
    /// Get order status by ID
    /// </summary>
    [HttpGet("{orderId:int}/get-order-status/")]
    public async Task<IActionResult> GetOrderStatusByIdAsync(int orderId, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the order exists
            var order = await _orderRepository.GetOrderByIdForEmployeeAsync(orderId, cancellationToken);
            if (order is null)
            {
                return NotFound(new { error = "Order not found" });
            }

            // Get the order status by ID
            var orderStatus = await _orderRepository.GetOrderStatusByIdAsync(orderId, cancellationToken);
            
            // Return the order status
            return Ok(orderStatus);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get order status display name by ID
    /// </summary>
    [HttpGet("{orderId:int}/get-order-status-display-name/")]
    public async Task<IActionResult> GetOrderStatusDisplayNameByIdAsync(int orderId, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the order exists
            var order = await _orderRepository.GetOrderByIdForEmployeeAsync(orderId, cancellationToken);
            if (order is null)
            {
                return NotFound(new { error = "Order not found" });
            }

            // Get the order status display name by ID
            var orderStatusDisplayName = await _orderRepository.GetOrderStatusDisplayNameByIdAsync(orderId, cancellationToken);
            
            // Return the order status display name
            return Ok(orderStatusDisplayName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Check if an order is locked
    /// </summary>
    [HttpGet("{orderId:int}/check-if-locked/")]
    public async Task<IActionResult> CheckIfOrderIsLockedAsync(int orderId, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the order exists
            var order = await _orderRepository.GetOrderByIdForEmployeeAsync(orderId, cancellationToken);
            if (order is null)
            {
                return NotFound(new { error = "Order not found" });
            }

            // Check if the order is locked
            var isLocked = await _orderRepository.CheckIfOrderIsLockedAsync(orderId, cancellationToken);

            // Return the result
            return Ok(isLocked);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}