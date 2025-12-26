using Kismet.Core.Helpers;
using Kismet.Entities.DTOs;
using Kismet.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kismet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ISessionRepository _sessionRepository;

    private const int minAccessLevel = 5;

    public CustomerController(ICustomerRepository customerRepository, 
                              IEmployeeRepository employeeRepository,
                              ISessionRepository sessionRepository)
    {
        _customerRepository = customerRepository;
        _employeeRepository = employeeRepository;
        _sessionRepository = sessionRepository;
    }

    /// <summary>
    /// Get all customers - Returns DTOs mapped from raw SQL query results
    /// </summary>
    [HttpGet("all-customers")]
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
    [HttpGet("customerId/{id:int}")]
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
    /// Get customer by email
    /// </summary>
    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        try
        {
            var customer = await _customerRepository.GetByEmailAsync(email, cancellationToken);
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
    /// Get customer by CustomerNumber
    /// </summary>
    [HttpGet("customer-number/{customerNumber}")]
    public async Task<IActionResult> GetByCustomerNumberAsync(string customerNumber, CancellationToken cancellationToken)
    {
        try
        {
            var customer = await _customerRepository.GetByCustomerNumberAsync(customerNumber, cancellationToken);
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

            // Create a new session for the customer and login
            var newSession = await _sessionRepository.CreateAsync(new CreateSessionDto
            {
                UserID = created.CustomerID,
                Email = created.ContactEmail,
                RefreshToken = string.Empty,
                RTExpiresAt = DateTimeOffset.UtcNow
            }, cancellationToken);
            
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
    [HttpPut("{customerId:int}/type")]
    public async Task<IActionResult> UpdateTypeAsync(int customerId, [FromBody] UpdateCustomerTypeDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dto.CustomerID = customerId; // Ensure ID matches route parameter
            
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
    [HttpPut("{employeeId:int}/{customerId:int}/reliability")]
    public async Task<IActionResult> UpdateReliabilityAsync(int employeeId, int customerId, [FromBody] UpdateCustomerReliabilityDto dto, CancellationToken cancellationToken)
    {
        try
        {
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
                return BadRequest(new { error = "Employee does not have permission to update customer reliability" });
            }

            dto.CustomerID = customerId; // Ensure ID matches route parameter
            
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
    /// Update only City and Country
    /// </summary>
    [HttpPut("{customerId:int}/city-country")]
    public async Task<IActionResult> UpdateCityAndCountryAsync(int customerId, [FromBody] UpdateCustomerCityAndCountryDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (dto.City is null && dto.Country is null)
            {
                return BadRequest(new { error = "At least one of City or Country must be provided" });
            }

            dto.CustomerID = customerId; // Ensure ID matches route parameter
            
            var updated = await _customerRepository.UpdateCustomerCityAndCountryAsync(dto, cancellationToken);
            
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
    [HttpDelete("{customerId:int}")]
    public async Task<IActionResult> DeleteAsync(int customerId, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _customerRepository.DeleteAsync(customerId, cancellationToken);
            
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

