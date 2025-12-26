using Kismet.Entities.DTOs;
using Kismet.Entities.Enums;
using Kismet.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kismet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BillingController : ControllerBase
{
    private readonly IBillingRepository _billingRepository;
    private readonly ICustomerRepository _customerRepository;

    public BillingController(IBillingRepository billingRepository, 
                           ICustomerRepository customerRepository)
    {
        _billingRepository = billingRepository;
        _customerRepository = customerRepository;
    }

    /// <summary>
    /// Get all billings of a customer to be seen by the customer
    /// </summary>
    [HttpGet("{customerId:int}/all-billings/for-customers")]
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

            // Get all billings for the customer
            var billings = await _billingRepository.GetBillingForCustomerAsync(customerId, cancellationToken);
            
            // Check if no billings found
            if (billings.Count == 0)
            {
                return NotFound(new { error = "No billings found" });
            }

            // Return the billings
            return Ok(billings);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all billings for employees to see
    /// </summary>
    [HttpGet("{customerId:int}/all-billings/for-employees")]
    public async Task<IActionResult> GetBillingByCustomerIdForEmployeeAsync(int customerId, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the customer exists
            var customer = await _customerRepository.GetByIdDtoAsync(customerId, cancellationToken);
            if (customer is null)
            {
                return NotFound(new { error = "Customer not found" });
            }

            // Get all billings for the customer
            var billings = await _billingRepository.GetBillingByCustomerIdForEmployeeAsync(customerId, cancellationToken);
            
            // Check if no billings found
            if (billings.Count == 0)
            {
                return NotFound(new { error = "No billings found" });
            }

            // Return the billings
            return Ok(billings);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get billing by ID for customers to see
    /// </summary>
    [HttpGet("{customerId:int}/{billingId:int}/get-by-billing-id/for-customers")]
    public async Task<IActionResult> GetBillingByIdForCustomerAsync(int customerId, int billingId, CancellationToken cancellationToken)
    {
        try
        {

            // Check if the customer exists
            var customer = await _customerRepository.GetByIdDtoAsync(customerId, cancellationToken);
            if (customer is null)
            {
                return NotFound(new { error = "Customer not found" });
            }

            // Get the billing by ID
            var billing = await _billingRepository.GetBillingByIdForCustomerAsync(customerId, billingId,cancellationToken);
            
            // Check if billing not found
            if (billing is null)
            {
                return NotFound(new { error = "Billing not found" });
            }

            // Return the billing
            return Ok(billing);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get billing by ID for employees to see
    /// </summary>
    [HttpGet("{billingId:int}/get-by-billing-id/for-employees")]
    public async Task<IActionResult> GetBillingByIdForEmployeeAsync(int billingId, CancellationToken cancellationToken)
    {
        try
        {
            // Get the billing by ID
            var billing = await _billingRepository.GetBillingByIdForEmployeeAsync(billingId, cancellationToken);
            
            // Check if billing not found
            if (billing is null)
            {
                return NotFound(new { error = "Billing not found" });
            }

            // Return the billing
            return Ok(billing);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new billing for a customer
    /// </summary>
    [HttpPost("{customerId:int}/create-billing")]
    public async Task<IActionResult> CreateBillingAsync(int customerId, [FromBody] CreateBillingDto dto, CancellationToken cancellationToken)
    {
        try
        {
            // Check if model state is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the customer exists before creating the billing
            var customer = await _customerRepository.GetByIdDtoAsync(customerId, cancellationToken);
            if (customer is null)
            {
                return NotFound(new { error = "Customer not found" });
            }

            // Set the customer ID
            dto.CustomerID = customerId;

            // Create the billing
            var billing = await _billingRepository.CreateBillingAsync(dto, cancellationToken);

            // Return the billing
            return Ok(billing);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update the type of a billing
    /// </summary>
    [HttpPut("{billingId:int}/update-billing-type/")]
    public async Task<IActionResult> UpdateBillingTypeAsync(int billingId, [FromBody] UpdateBillingTypeDto dto, CancellationToken cancellationToken)
    {
        try
        {
            // Check if model state is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dto.BillingID = billingId;

            // Check if the billing exists
            var billing = await _billingRepository.GetBillingByIdForEmployeeAsync(billingId, cancellationToken);
            if (billing is null)
            {
                return NotFound(new { error = "Billing not found" });
            }

            // Update the billing type
            var updatedBilling = await _billingRepository.UpdateBillingTypeAsync(dto, cancellationToken);

            // Return the updated billing
            return Ok(updatedBilling);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Total due and total paid update for a billing in case of a payment
    /// </summary>
    [HttpPut("{customerId:int}/{billingId:int}/update-billing/")]
    public async Task<IActionResult> UpdateBillingAsync(int customerId, int billingId, /*int paymentId,*/ [FromBody] UpdateBillingDto dto, CancellationToken cancellationToken)
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

            // Check if the billing exists
            var billing = await _billingRepository.GetBillingByIdForCustomerAsync(customerId, billingId, cancellationToken);
            if (billing is null)
            {
                return NotFound(new { error = "Billing not found" });
            }

            // Check if the payment exists
            //aaa var payment = await _paymentRepository.GetPaymentByIdAsync(paymentId, cancellationToken);
            //if (payment is null)
            //{
            //    return NotFound(new { error = "Payment not found" });
            //}

            // Check if both total due and total paid are 0 or null
            if ((dto.TotalDue is null && dto.TotalPaid is null) || (dto.TotalDue == 0 && dto.TotalPaid == 0))
            {
                return BadRequest(new { error = "Both total due and total paid cannot be 0 or null" });
            }
            
            // Check if the new TotalDue is NOT less than the current TotalDue
            else if (dto.TotalDue != 0 && dto.TotalDue <= billing.TotalDue)
            {
                return BadRequest(new { error = "New total due must be greater than the current total due" });
            }
            
            // Update the billing total due and total paid
            var updatedBilling = await _billingRepository.UpdateBillingAsync(customerId, billingId, /*paymentId,*/ dto, cancellationToken);

            // Return the billing
            return Ok(updatedBilling);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update the payment terms of a billing
    /// </summary>
    [HttpPut("{billingId:int}/update-billing-payment-terms/")]
    public async Task<IActionResult> UpdateBillingPaymentTermsAsync(int billingId, [FromBody] UpdateBillingPaymentTermsDto dto, CancellationToken cancellationToken)
    {
        try
        {
            // Check if model state is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dto.BillingID = billingId;

            // Check if the billing exists
            var billing = await _billingRepository.GetBillingByIdForEmployeeAsync(billingId, cancellationToken);
            if (billing is null)
            {
                return NotFound(new { error = "Billing not found" });
            }

            // Update the billing payment terms
            var updatedBilling = await _billingRepository.UpdateBillingPaymentTermsAsync(dto, cancellationToken);

            // Return the updated billing
            return Ok(updatedBilling);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get billing status by ID
    /// </summary>
    [HttpGet("{billingId:int}/get-billing-status/")]
    public async Task<IActionResult> GetBillingStatusByIdAsync(int billingId, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the billing exists
            var billing = await _billingRepository.GetBillingByIdForEmployeeAsync(billingId, cancellationToken);
            if (billing is null)
            {
                return NotFound(new { error = "Billing not found" });
            }

            // Get the billing status by ID
            var billingStatus = await _billingRepository.GetBillingStatusByIdAsync(billingId, cancellationToken);
            
            // Return the billing status
            return Ok(billingStatus);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get billing status display name by ID
    /// </summary>
    [HttpGet("{billingId:int}/get-billing-status-display-name/")]
    public async Task<IActionResult> GetBillingStatusDisplayNameByIdAsync(int billingId, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the billing exists
            var billing = await _billingRepository.GetBillingByIdForEmployeeAsync(billingId, cancellationToken);
            if (billing is null)
            {
                return NotFound(new { error = "Billing not found" });
            }

            // Get the billing status display name by ID
            var billingStatusDisplayName = await _billingRepository.GetBillingStatusByIdAsync(billingId, cancellationToken);
            
            // Return the billing status display name
            return Ok(billingStatusDisplayName.GetDisplayName());
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}