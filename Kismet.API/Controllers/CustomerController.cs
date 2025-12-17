using Kismet.Entities.DTOs;
using Kismet.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kismet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerController(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    /// <summary>
    /// Get all customers - Returns DTOs mapped from raw SQL query results
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        try
        {
            var customers = await _customerRepository.GetAllDtoAsync(cancellationToken);

            if (customers.Count == 0)
            {
                return NotFound(new { error = "No customers found" });
            }
            return Ok(customers);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get customer by ID - Returns DTO mapped from raw SQL query result
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var customer = await _customerRepository.GetByIdDtoAsync(id, cancellationToken);
            
            if (customer is null)
            {
                return NotFound(new { error = "Customer not found" });
            }

            return Ok(customer);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new customer - Accepts DTO, Dapper maps DTO properties to SQL parameters
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateCustomerDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var created = await _customerRepository.CreateAsync(dto, cancellationToken);
            
            // Return created customer
            return Created($"/api/customers/{created.CustomerID}", created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update CustomerType
    /// </summary>
    [HttpPut("{id:int}/type")]
    public async Task<IActionResult> UpdateTypeAsync(int id, [FromBody] UpdateCustomerTypeDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dto.CustomerID = id; // Ensure ID matches route parameter
            
            var updated = await _customerRepository.UpdateCustomerTypeAsync(dto, cancellationToken);
            
            if (updated is null)
            {
                return NotFound(new { error = "Customer not found" });
            }

            return Ok(updated);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update customer reliability only
    /// </summary>
    [HttpPut("{id:int}/reliability")]
    public async Task<IActionResult> UpdateReliabilityAsync(int id, [FromBody] UpdateCustomerReliabilityDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dto.CustomerID = id; // Ensure ID matches route parameter
            
            var updated = await _customerRepository.UpdateCustomerReliabilityAsync(dto, cancellationToken);
            
            if (updated is null)
            {
                return NotFound(new { error = "Customer not found" });
            }

            return Ok(updated);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete customer
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _customerRepository.DeleteAsync(id, cancellationToken);
            
            if (!deleted)
            {
                return NotFound(new { error = "Customer not found" });
            }

            return Ok(new { message = "Customer deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

