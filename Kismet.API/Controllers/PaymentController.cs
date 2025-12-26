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
    [HttpGet("{customerId:int}/get-all-customer-payments")]
    public async Task<IActionResult> GetAllCustomerPaymentsAsync(int customerId, CancellationToken cancellationToken)
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
            var payments = await _paymentRepository.GetAllCustomerPaymentsAsync(customerId, cancellationToken);
            
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
    [HttpGet("{customerId:int}/{paymentId:int}/get-customer-payment-by-id")]
    public async Task<IActionResult> GetCustomerPaymentByIdAsync(int customerId, int paymentId, CancellationToken cancellationToken)
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
            var payment = await _paymentRepository.GetCustomerPaymentByIdAsync(customerId, paymentId, cancellationToken);
            
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
    public async Task<IActionResult> CreatePaymentAsync(int customerId, int fTransactionId, [FromBody] CreatePaymentDto dto, CancellationToken cancellationToken)
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

            // Check if the Financial Transaction exists
            var financialTransaction = await _financialTransactionRepository.GetFTByIdForEmployeeAsync(fTransactionId, cancellationToken);
            if (financialTransaction is null)
            {
                return NotFound(new { error = "Financial transaction not found" });
            }

            dto.FTransactionID = fTransactionId;

            // Check if the FT belongs to the customer
            if (financialTransaction.CustomerID != customerId)
            {
                return BadRequest(new { error = "Financial transaction does not belong to the customer" });
            }

            // Check if Billing, FT and Payment types are compatible
            if (dto.PaymentType != financialTransaction.TransactionType)
            {
                return BadRequest(new { error = "Billing, Financial transaction and Payment types are not compatible" });
            }

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