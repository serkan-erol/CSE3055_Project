using Kismet.Core.Helpers;
using Kismet.Entities.DTOs;
using Kismet.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kismet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly JwtHelper _jwtHelper;

    public EmployeeController(IEmployeeRepository employeeRepository, 
                              ISessionRepository sessionRepository, 
                              JwtHelper jwtHelper)
    {
        _employeeRepository = employeeRepository;
        _sessionRepository = sessionRepository;
        _jwtHelper = jwtHelper;
    }

    /// <summary>
    /// Get all employees - Returns DTOs mapped from raw SQL query results
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        try
        {
            var employees = await _employeeRepository.GetAllDtoAsync(cancellationToken);

            if (employees.Count == 0)
            {
                return NotFound(new { error = "No employees found" });
            }
            return Ok(employees);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get employee by ID - Returns DTO mapped from raw SQL query result
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var employee = await _employeeRepository.GetByIdDtoAsync(id, cancellationToken);
            
            if (employee is null)
            {
                return NotFound(new { error = "Employee not found" });
            }

            return Ok(employee);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new employee - Accepts DTO, Dapper maps DTO properties to SQL parameters
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateEmployeeDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var created = await _employeeRepository.CreateAsync(dto, cancellationToken);

            // Create a new session for the employee and login
            var newSession = await _sessionRepository.CreateAsync(new CreateSessionDto
            {
                UserID = created.EmployeeID,
                Email = created.ContactEmail,
                RefreshToken = string.Empty,
                RTExpiresAt = DateTimeOffset.UtcNow
            }, cancellationToken);
            
            // Return Created with Location header pointing to the new resource
            return Created($"/api/employees/{created.EmployeeID}", created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update employee role
    /// </summary>
    [HttpPut("{id:int}/role")]
    public async Task<IActionResult> UpdateEmployeeRoleAsync(int id, [FromBody] UpdateEmployeeRoleDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dto.EmployeeID = id; // Ensure ID matches route parameter

            var updated = await _employeeRepository.UpdateEmployeeRoleAsync(dto, cancellationToken);
            
            if (updated is null)
            {
                return NotFound(new { error = "Employee not found" });
            }

            return Ok(updated);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update employee access level
    /// </summary>
    [HttpPut("{id:int}/access-level")]
    public async Task<IActionResult> UpdateEmployeeAccessLevelAsync(int id, [FromBody] UpdateEmployeeAccessLevelDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dto.EmployeeID = id; // Ensure ID matches route parameter
            
            var updated = await _employeeRepository.UpdateEmployeeAccessLevelAsync(dto, cancellationToken);
            
            if (updated is null)
            {
                return NotFound(new { error = "Employee not found" });
            }

            return Ok(updated);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete employee
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _employeeRepository.DeleteAsync(id, cancellationToken);
            
            if (!deleted)
            {
                return NotFound(new { error = "Employee not found" });
            }

            return Ok(new { message = "Employee deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

