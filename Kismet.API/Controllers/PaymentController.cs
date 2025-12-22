using Kismet.Entities.DTOs;
using Kismet.Entities.Enums;
using Kismet.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kismet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBillingRepository _billingRepository;
    private readonly IFinancialTransactionRepository _financialTransactionRepository;
    private readonly ICustomerRepository _customerRepository;

    public PaymentController(IPaymentRepository paymentRepository,
                             IBillingRepository billingRepository,
                             IFinancialTransactionRepository financialTransactionRepository,
                             ICustomerRepository customerRepository)
    {
        _paymentRepository = paymentRepository;
        _billingRepository = billingRepository;
        _financialTransactionRepository = financialTransactionRepository;
        _customerRepository = customerRepository;
    }

    /// <summary>
    /// Get all payments
    /// </summary>
    [HttpGet("get-all-payments/")]
    public async Task<IActionResult> GetAllPaymentsAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Get all payments
            var payments = await _paymentRepository.GetAllPaymentsAsync(cancellationToken);
            
            // Return the payments
            return Ok(payments);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get a payment by ID
    /// </summary>
    [HttpGet("{paymentId:int}/get-by-payment-id/")]
    public async Task<IActionResult> GetPaymentByIdAsync(int paymentId, CancellationToken cancellationToken)
    {
        try
        {
            // Get the payment by ID
            var payment = await _paymentRepository.GetPaymentByIdAsync(paymentId, cancellationToken);
            
            // Return the payment
            return Ok(payment);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all payments for a customer
    /// </summary>
    [HttpGet("{customerId:int}/get-all-payments/for-customers")]
    public async Task<IActionResult> GetPaymentForCustomerAsync(int customerId, CancellationToken cancellationToken)
    {
        try
        {

            // Check if the customer exists
            var customer = await _customerRepository.GetByIdDtoAsync(customerId, cancellationToken);
            if (customer is null)
            {
                return NotFound(new { error = "Customer not found" });
            }

            // Get all payments for the customer
            var payments = await _paymentRepository.GetPaymentForCustomerAsync(customerId, cancellationToken);
            
            // Return the payments
            return Ok(payments);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get a payment by ID for a customer
    /// </summary>
    [HttpGet("{customerId:int}/{paymentId:int}/get-by-payment-id/for-customers")]
    public async Task<IActionResult> GetPaymentByIdForCustomerAsync(int customerId, int paymentId, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the customer exists
            var customer = await _customerRepository.GetByIdDtoAsync(customerId, cancellationToken);
            if (customer is null)
            {
                return NotFound(new { error = "Customer not found" });
            }

            // Get the payment by ID for the customer
            var payment = await _paymentRepository.GetPaymentByIdForCustomerAsync(customerId, paymentId, cancellationToken);
            
            // Check if the payment exists
            if (payment is null)
            {
                return NotFound(new { error = "Payment not found" });
            }

            // Return the payment
            return Ok(payment);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new payment
    /// </summary>
    [HttpPost("create-payment/")]
    public async Task<IActionResult> CreatePaymentAsync(int billingId, int fTransactionId, [FromBody] CreatePaymentDto dto, CancellationToken cancellationToken)
    {
        try
        {
            // Check if model state is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the Billing exists
            var billing = await _billingRepository.GetBillingByIdForEmployeeAsync(billingId, cancellationToken);
            if (billing is null)
            {
                return NotFound(new { error = "Billing not found" });
            }

            dto.BillingID = billingId;

            // Check if the Financial Transaction exists
            var financialTransaction = await _financialTransactionRepository.GetFTByIdForEmployeeAsync(fTransactionId, cancellationToken);
            if (financialTransaction is null)
            {
                return NotFound(new { error = "Financial transaction not found" });
            }

            dto.FTransactionID = fTransactionId;

            // Create the payment
            var payment = await _paymentRepository.CreatePaymentAsync(dto, cancellationToken);

            // Return the payment
            return Ok(payment);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}