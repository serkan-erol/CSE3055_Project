using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Kismet.Entities.DTOs;
using Kismet.Repository.Interfaces;
using System.Security.Claims;

namespace Kismet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerPaymentController : ControllerBase
{
    private readonly ICustomerPaymentRepository _customerPaymentRepository;
    private readonly ILogger<CustomerPaymentController> _logger;

    public CustomerPaymentController(
        ICustomerPaymentRepository customerPaymentRepository,
        ILogger<CustomerPaymentController> logger)
    {
        _customerPaymentRepository = customerPaymentRepository;
        _logger = logger;
    }

    // Saved Payment Methods

    /// <summary>
    /// Get all saved payment methods for a customer
    /// </summary>
    [HttpGet("customers/{customerId}/payment-methods")]
    public async Task<IActionResult> GetSavedPaymentMethodsByCustomerId(
        int customerId,
        CancellationToken cancellationToken)
    {
        try
        {
            var paymentMethods = await _customerPaymentRepository.GetSavedPaymentMethodsByCustomerIdAsync(customerId, cancellationToken);
            return Ok(paymentMethods);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving saved payment methods for customer {CustomerId}", customerId);
            return StatusCode(500, "An error occurred while retrieving saved payment methods");
        }
    }

    /// <summary>
    /// Get a specific saved payment method by ID
    /// </summary>
    [HttpGet("payment-methods/{spmId}")]
    public async Task<IActionResult> GetSavedPaymentMethodById(
        int spmId,
        CancellationToken cancellationToken)
    {
        try
        {
            var paymentMethod = await _customerPaymentRepository.GetSavedPaymentMethodByIdAsync(spmId, cancellationToken);
            
            if (paymentMethod == null)
            {
                return NotFound();
            }

            return Ok(paymentMethod);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving saved payment method {SPMId}", spmId);
            return StatusCode(500, "An error occurred while retrieving the saved payment method");
        }
    }

    /// <summary>
    /// Create a new saved payment method
    /// </summary>
    [HttpPost("payment-methods")]
    public async Task<IActionResult> CreateSavedPaymentMethod(
        [FromBody] CreateSavedPaymentMethodDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate CardType
            if (dto.CardType != "Debit" && dto.CardType != "Credit")
            {
                return BadRequest(new { message = "CardType must be either 'Debit' or 'Credit'" });
            }

            var paymentMethod = await _customerPaymentRepository.CreateSavedPaymentMethodAsync(dto, cancellationToken);
            
            return CreatedAtAction(
                nameof(GetSavedPaymentMethodById),
                new { spmId = paymentMethod.SPMID },
                paymentMethod);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating saved payment method for customer {CustomerId}", dto.CustomerID);
            return StatusCode(500, "An error occurred while creating the saved payment method");
        }
    }

    /// <summary>
    /// Update an existing saved payment method
    /// </summary>
    [HttpPut("payment-methods/{spmId}")]
    public async Task<IActionResult> UpdateSavedPaymentMethod(
        int spmId,
        [FromBody] UpdateSavedPaymentMethodDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate CardType
            if (dto.CardType != "Debit" && dto.CardType != "Credit")
            {
                return BadRequest(new { message = "CardType must be either 'Debit' or 'Credit'" });
            }

            var paymentMethod = await _customerPaymentRepository.UpdateSavedPaymentMethodAsync(spmId, dto, cancellationToken);
            
            return Ok(paymentMethod);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating saved payment method {SPMId}", spmId);
            return StatusCode(500, "An error occurred while updating the saved payment method");
        }
    }

    /// <summary>
    /// Delete a saved payment method
    /// </summary>
    [HttpDelete("payment-methods/{spmId}")]
    public async Task<IActionResult> DeleteSavedPaymentMethod(
        int spmId,
        CancellationToken cancellationToken)
    {
        try
        {
            var success = await _customerPaymentRepository.DeleteSavedPaymentMethodAsync(spmId, cancellationToken);
            
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting saved payment method {SPMId}", spmId);
            return StatusCode(500, "An error occurred while deleting the saved payment method");
        }
    }

    // Saved Bank Information

    /// <summary>
    /// Get all saved bank information for a customer
    /// </summary>
    [HttpGet("customers/{customerId}/bank-information")]
    public async Task<IActionResult> GetSavedBankInformationByCustomerId(
        int customerId,
        CancellationToken cancellationToken)
    {
        try
        {
            var bankInfo = await _customerPaymentRepository.GetSavedBankInformationByCustomerIdAsync(customerId, cancellationToken);
            return Ok(bankInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving saved bank information for customer {CustomerId}", customerId);
            return StatusCode(500, "An error occurred while retrieving saved bank information");
        }
    }

    /// <summary>
    /// Get a specific saved bank information by ID
    /// </summary>
    [HttpGet("bank-information/{sbiId}")]
    public async Task<IActionResult> GetSavedBankInformationById(
        int sbiId,
        CancellationToken cancellationToken)
    {
        try
        {
            var bankInfo = await _customerPaymentRepository.GetSavedBankInformationByIdAsync(sbiId, cancellationToken);
            
            if (bankInfo == null)
            {
                return NotFound();
            }

            return Ok(bankInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving saved bank information {SBIId}", sbiId);
            return StatusCode(500, "An error occurred while retrieving the saved bank information");
        }
    }

    /// <summary>
    /// Create new saved bank information
    /// </summary>
    [HttpPost("bank-information")]
    public async Task<IActionResult> CreateSavedBankInformation(
        [FromBody] CreateSavedBankInformationDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bankInfo = await _customerPaymentRepository.CreateSavedBankInformationAsync(dto, cancellationToken);
            
            return CreatedAtAction(
                nameof(GetSavedBankInformationById),
                new { sbiId = bankInfo.SBIID },
                bankInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating saved bank information for customer {CustomerId}", dto.CustomerID);
            return StatusCode(500, "An error occurred while creating the saved bank information");
        }
    }

    /// <summary>
    /// Update an existing saved bank information
    /// </summary>
    [HttpPut("bank-information/{sbiId}")]
    public async Task<IActionResult> UpdateSavedBankInformation(
        int sbiId,
        [FromBody] UpdateSavedBankInformationDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bankInfo = await _customerPaymentRepository.UpdateSavedBankInformationAsync(sbiId, dto, cancellationToken);
            
            return Ok(bankInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating saved bank information {SBIId}", sbiId);
            return StatusCode(500, "An error occurred while updating the saved bank information");
        }
    }

    /// <summary>
    /// Delete saved bank information
    /// </summary>
    [HttpDelete("bank-information/{sbiId}")]
    public async Task<IActionResult> DeleteSavedBankInformation(
        int sbiId,
        CancellationToken cancellationToken)
    {
        try
        {
            var success = await _customerPaymentRepository.DeleteSavedBankInformationAsync(sbiId, cancellationToken);
            
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting saved bank information {SBIId}", sbiId);
            return StatusCode(500, "An error occurred while deleting the saved bank information");
        }
    }
}
