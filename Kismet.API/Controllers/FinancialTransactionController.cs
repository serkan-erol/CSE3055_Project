using Kismet.Entities.DTOs;
using Kismet.Entities.Enums;
using Kismet.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kismet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FinancialTransactionController : ControllerBase
{
    private readonly IFinancialTransactionRepository _financialTransactionRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IBillingRepository _billingRepository;
    private readonly IOrderRepository _orderRepository;

    private const int minAccessLevel = 5;

    public FinancialTransactionController(IFinancialTransactionRepository financialTransactionRepository, 
                                          ICustomerRepository customerRepository,
                                          IEmployeeRepository employeeRepository,
                                          IBillingRepository billingRepository,
                                          IOrderRepository orderRepository)
    {
        _financialTransactionRepository = financialTransactionRepository;
        _customerRepository = customerRepository;
        _employeeRepository = employeeRepository;
        _billingRepository = billingRepository;
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// Get all financial transactions of a customer to be seen by the customer
    /// </summary>
    [HttpGet("{customerId:int}/get-all-customer-financial-transactions/for-customers")]
    public async Task<IActionResult> GetAllFTsForCustomerAsync(int customerId, CancellationToken cancellationToken)
    {
        try
        {
            // Check if customer exists
            var customer = await _customerRepository.GetByIdDtoAsync(customerId, cancellationToken);
            if (customer is null)
            {
                return NotFound(new { error = "Customer not found" });
            }

            // Get all financial transactions for the customer
            var financialTransactions = await _financialTransactionRepository.GetFTForCustomerAsync(customerId, cancellationToken);
            
            // Check if no financial transactions found
            if (financialTransactions.Count == 0)
            {
                return NotFound(new { error = "No financial transactions found" });
            }

            // Return the financial transactions
            return Ok(financialTransactions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all financial transactions of a customer for employees to see
    /// </summary>
    [HttpGet("{employeeId:int}/{customerId:int}/get-all-customer-financial-transactions/for-employees")]
    public async Task<IActionResult> GetAllFTsByCustomerIdForEmployeeAsync(int employeeId, int customerId, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the employee exists
            var employee = await _employeeRepository.GetByIdDtoAsync(employeeId, cancellationToken);
            if (employee is null)
            {
                return NotFound(new { error = "Employee not found" });
            }

            // Check if the employee's access level is at least 5
            if (employee.AccessLevel < minAccessLevel)
            {
                return BadRequest(new { error = "Employee does not have permission to get all financial transactions for customers" });
            }

            // Check if the customer exists
            var customer = await _customerRepository.GetByIdDtoAsync(customerId, cancellationToken);
            if (customer is null)
            {
                return NotFound(new { error = "Customer not found" });
            }

            // Get all financial transactions for the customer
            var financialTransactions = await _financialTransactionRepository.GetFTByCustomerIdForEmployeeAsync(customerId, cancellationToken);
            
            // Check if no financial transactions found
            if (financialTransactions.Count == 0)
            {
                return NotFound(new { error = "No financial transactions found" });
            }

            // Return the financial transactions
            return Ok(financialTransactions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get ALL financial transactions for employees to see
    /// </summary>
    [HttpGet("{employeeId:int}/get-all-financial-transactions/for-employees")]
    public async Task<IActionResult> GetAllFTsForEmployeeAsync(int employeeId, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the employee exists
            var employee = await _employeeRepository.GetByIdDtoAsync(employeeId, cancellationToken);
            if (employee is null)
            {
                return NotFound(new { error = "Employee not found" });
            }

            // Check if the employee's access level is at least 5
            if (employee.AccessLevel < minAccessLevel)
            {
                return BadRequest(new { error = "Employee does not have permission to get all financial transactions" });
            }

            // Get all financial transactions for the employees
            var financialTransactions = await _financialTransactionRepository.GetFTForEmployeeAsync(cancellationToken);
            
            // Check if no financial transactions found
            if (financialTransactions.Count == 0)
            {
                return NotFound(new { error = "No financial transactions found" });
            }

            // Return the financial transactions
            return Ok(financialTransactions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get financial transaction by ID for customers to see
    /// </summary>
    [HttpGet("{customerId:int}/{fTransactionId:int}/get-by-fTransaction-id/for-customers")]
    public async Task<IActionResult> GetFTByIdForCustomerAsync(int customerId, int fTransactionId, CancellationToken cancellationToken)
    {
        try
        {

            // Check if the customer exists
            var customer = await _customerRepository.GetByIdDtoAsync(customerId, cancellationToken);
            if (customer is null)
            {
                return NotFound(new { error = "Customer not found" });
            }

            // Get the financial transaction by ID
            var financialTransaction = await _financialTransactionRepository.GetFTByIdForCustomerAsync(customerId, fTransactionId,cancellationToken);
            
            // Check if financial transaction not found
            if (financialTransaction is null)
            {
                return NotFound(new { error = "Financial transaction not found" });
            }

            // Return the financial transaction
            return Ok(financialTransaction);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get financial transaction by ID for employees to see
    /// </summary>
    [HttpGet("{employeeId:int}/{fTransactionId:int}/get-by-fTransaction-id/for-employees")]
    public async Task<IActionResult> GetFTByIdForEmployeeAsync(int employeeId, int fTransactionId, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the employee exists
            var employee = await _employeeRepository.GetByIdDtoAsync(employeeId, cancellationToken);
            if (employee is null)
            {
                return NotFound(new { error = "Employee not found" });
            }

            // Check if the employee's access level is at least 5
            if (employee.AccessLevel < minAccessLevel)
            {
                return BadRequest(new { error = "Employee does not have permission to get financial transaction by ID" });
            }

            // Get the financial transaction by ID
            var financialTransaction = await _financialTransactionRepository.GetFTByIdForEmployeeAsync(fTransactionId, cancellationToken);
            
            // Check if financial transaction not found
            if (financialTransaction is null)
            {
                return NotFound(new { error = "Financial transaction not found" });
            }

            // Return the financial transaction
            return Ok(financialTransaction);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get PaymentStatus of a FT
    /// </summary>
    [HttpGet("{fTransactionId:int}/get-payment-status/")]
    public async Task<IActionResult> GetFTPaymentStatusAsync(int fTransactionId, CancellationToken cancellationToken)
    {
        try
        {
            // Get the payment status by ID
            var paymentStatus = await _financialTransactionRepository.GetFTPaymentStatusAsync(fTransactionId, cancellationToken);
            return Ok(new FTPaymentStatusResponseDto { PaymentStatus = paymentStatus });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get PaymentStatus display name by ID
    /// </summary>
    [HttpGet("{fTransactionId:int}/get-payment-status-display-name/")]
    public async Task<IActionResult> GetPaymentStatusDisplayNameAsync(int fTransactionId, CancellationToken cancellationToken)
    {
        try
        {
            // Get the payment status by ID
            var ft = await _financialTransactionRepository.GetFTPaymentStatusAsync(fTransactionId, cancellationToken);
            
            // Return the payment status display name
            return Ok(ft.GetDisplayName());
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new financial transaction for a customer
    /// </summary>
    [HttpPost("{customerId:int}/{orderId:int}/create-financial-transaction")]
    public async Task<IActionResult> CreateFTAsync(int customerId, int orderId, [FromBody] CreateFTDto dto, CancellationToken cancellationToken)
    {
        try
        {
            // Check if model state is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the customer exists
            var customer = await _customerRepository.GetByIdDtoAsync(customerId, cancellationToken);
            if (customer is null)
            {
                return NotFound(new { error = "Customer not found" });
            }
            
            // Set the customer ID in the DTO
            dto.CustomerID = customerId;

            // Check if the order exists
            var order = await _orderRepository.GetOrderByIdForCustomerAsync(customerId, orderId, cancellationToken);
            
            // Check if the order exists
            if (order is null)
            {
                return NotFound(new { error = "Order not found" });
            }

            // Set the order ID in the DTO
            dto.OrderID = orderId;

            // Check if the BillingID is set in the DTO
            // Normally, we do NOT plan to set the BillingID in the DTO.
            // This redundancy is to handle the case where we decide to set the BillingID in the DTO for some reason.
            if (dto.BillingID != 0)
            {
                // Check if the Billing entry exists and it is suitable for the FT
                var isSuitable = await _financialTransactionRepository.IsBillingEntrySuitableForFTAsync(dto, cancellationToken);
                
                // If the Billing entry is not suitable, create a new Billing entry for the FT
                if (!isSuitable)
                {
                    //aaa return NotFound(new { error = "Associated Billing not found" });
                    // Set values for a new Billing entry
                    var newBillingDto = new CreateBillingDto
                    {
                        CustomerID = customerId,
                        BillingType = dto.TransactionType,
                        InvoiceNumber = Guid.NewGuid().ToString(), // Generate a unique invoice number
                        TotalDue = dto.TotalAmount,
                        BillingDate = dto.TransactionDate,
                    };

                    // Create the new Billing entry and set the BillingID in the DTO
                    var newBilling = await _billingRepository.CreateBillingAsync(newBillingDto, cancellationToken);
                    dto.BillingID = newBilling.BillingID;
                }

                // If the given BillingID is suitable, update that Billing entry's total due
                else
                {
                    var updateBillingDto = new UpdateBillingDto
                    {
                        BillingID = dto.BillingID,
                        TotalDue = dto.TotalAmount,
                    };
                    await _billingRepository.UpdateBillingAsync(customerId, dto.BillingID, updateBillingDto, cancellationToken);
                }
            }
            
            // This is the default case where we do NOT set the BillingID in the DTO.
            if (dto.BillingID == 0)
            {
                // Check if there are any suitable Billing entries for the FT
                var suitableBillingEntry = await _financialTransactionRepository.FindSuitableBillingEntryForFTAsync(dto, cancellationToken);
                // If there is not any, create a new Billing entry for the FT
                if (suitableBillingEntry is null)
                {
                    //aaa return NotFound(new { error = "No suitable Billing entry found" });                

                    // Create a new Billing entry for the FT
                    var newBillingDto = new CreateBillingDto
                    {
                        CustomerID = customerId,
                        BillingType = dto.TransactionType,
                        InvoiceNumber = Guid.NewGuid().ToString(), // Generate a unique invoice number
                        TotalDue = dto.TotalAmount,
                        BillingDate = dto.TransactionDate,
                    };

                    // Create the new Billing entry and set the BillingID in the DTO
                    var newBilling = await _billingRepository.CreateBillingAsync(newBillingDto, cancellationToken);
                    dto.BillingID = newBilling.BillingID;
                }

                // If the given BillingID is suitable, set the BillingID in the DTO
                else
                {
                    dto.BillingID = suitableBillingEntry.BillingID;
                    var updateBillingDto = new UpdateBillingDto
                    {
                        BillingID = dto.BillingID,
                        TotalDue = dto.TotalAmount,
                    };
                    await _billingRepository.UpdateBillingAsync(customerId, dto.BillingID, updateBillingDto, cancellationToken);
                }
            }

            // Create the financial transaction
            var financialTransaction = await _financialTransactionRepository.CreateFTAsync(dto, cancellationToken);

            //aaa /OLD/ This will be un-commented when we implement the Treasury 
            //aaa Now, a trigger handles the creation of the Treasury entry for the FT.
            //aaa So, we do not need to create the Treasury entry here.
            // Create the treasury entry for the FT
            //var treasuryDto = new CreateTreasuryDto
            //{
            //    FTransactionID = financialTransaction.FTransactionID,
            //    Amount = financialTransaction.TotalAmount,
            //    Description = financialTransaction.Description,
            //};
            //var treasury = await _treasuryRepository.CreateTreasuryEntryAsync(treasuryDto, cancellationToken);

            // Return the financial transaction
            return Ok(financialTransaction);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update the TotalPaid of a FT
    /// </summary>
    [HttpPut("{customerId:int}/{fTransactionId:int}/update-total-paid/")]
    public async Task<IActionResult> UpdateFTTotalPaidAsync(int customerId, int fTransactionId, [FromBody] UpdateFTTotalPaidDto dto, CancellationToken cancellationToken)
    {
        try
        {
            // Check if model state is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the customer by ID
            var customer = await _customerRepository.GetByIdDtoAsync(customerId, cancellationToken);
            
            // Check if the customer exists
            if (customer is null)
            {
                return NotFound(new { error = "Customer not found" });
            }

            // Set the financial transaction ID in the DTO
            dto.FTransactionID = fTransactionId;

            // Get the financial transaction by ID for the customer
            var financialTransaction = await _financialTransactionRepository.GetFTByIdForCustomerAsync(customerId, fTransactionId, cancellationToken);
            
            // Check if the financial transaction exists
            if (financialTransaction is null)
            {
                return NotFound(new { error = "Financial transaction not found" });
            }

            // Update the financial transaction total paid
            var updatedFinancialTransaction = await _financialTransactionRepository.UpdateFTTotalPaidAsync(customerId, fTransactionId, dto, cancellationToken);

            // Return the updated financial transaction
            return Ok(updatedFinancialTransaction);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update the Description of a FT
    /// </summary>
    [HttpPut("{employeeId:int}/{fTransactionId:int}/update-description/")]
    public async Task<IActionResult> UpdateFTDescriptionAsync(int employeeId, int fTransactionId, [FromBody] UpdateFTDescriptionDto dto, CancellationToken cancellationToken)
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

            // Check if the employee's access level is at least 5
            if (employee.AccessLevel < minAccessLevel)
            {
                return BadRequest(new { error = "Employee does not have permission to update the description of a financial transaction" });
            }

            // Get the FT by ID
            var ft = await _financialTransactionRepository.GetFTByIdForEmployeeAsync(fTransactionId, cancellationToken);
            
            // Check if the FT exists
            if (ft is null)
            {
                return NotFound(new { error = "Financial transaction not found" });
            }

            // Set the FTransactionID in the DTO
            dto.FTransactionID = fTransactionId;

            // Update the FT description
            var updatedFT = await _financialTransactionRepository.UpdateFTDescriptionAsync(dto, cancellationToken);

            // Return the updated FT
            return Ok(updatedFT);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}