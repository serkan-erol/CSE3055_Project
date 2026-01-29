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

    private const int minAccessLevel = 5;
    private const int createEmployeeAccessLevel = 7;
    private const int updateEmployeeAccessLevel = 7;
    private const int deleteEmployeeAccessLevel = 7;

    public EmployeeController(IEmployeeRepository employeeRepository, 
                              ISessionRepository sessionRepository)
    {
        _employeeRepository = employeeRepository;
        _sessionRepository = sessionRepository;
    }

    /// <summary>
    /// Get all employees - Returns DTOs mapped from raw SQL query results
    /// </summary>
    [HttpGet("{employeeId:int}/all-employees")]
    public async Task<IActionResult> GetAsync(int employeeId, CancellationToken cancellationToken)
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
                return BadRequest(new { error = "Employee does not have permission to get all employees" });
            }

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
    [HttpGet("{employeeId:int}/employee-by-id")]
    public async Task<IActionResult> GetByIdAsync(int employeeId, CancellationToken cancellationToken)
    {
        try
        {
            var employee = await _employeeRepository.GetByIdDtoAsync(employeeId, cancellationToken);
            
            // Check if the employee exists
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
    /// Get employee by email
    /// </summary>
    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        try
        {
            var employee = await _employeeRepository.GetByEmailAsync(email, cancellationToken);
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
    /// Get employee by EmployeeNumber
    /// </summary>
    [HttpGet("employee-number/{employeeNumber}")]
    public async Task<IActionResult> GetByEmployeeNumberAsync(string employeeNumber, CancellationToken cancellationToken)
    {
        try
        {
            var employee = await _employeeRepository.GetByEmployeeNumberAsync(employeeNumber, cancellationToken);
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
    [HttpPost("{employeeId:int}/create-employee")]
    public async Task<IActionResult> CreateAsync(int employeeId, [FromBody] CreateEmployeeDto dto, CancellationToken cancellationToken)
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

            // Check if the employee's access level is at least 7
            if (employee.AccessLevel < createEmployeeAccessLevel)
            {
                return BadRequest(new { error = "Employee does not have permission to create employees" });
            }
            
            var created = await _employeeRepository.CreateAsync(dto, cancellationToken);

            // Note: Employee creation does not require an auto-login, so a session is NOT created
            
            // Return Created with Location header pointing to the new resource
            return Created($"/api/employees/{employeeId}/{created.EmployeeID}", created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update employee role
    /// </summary>
    [HttpPut("{updaterEmployeeId:int}/role")]
    public async Task<IActionResult> UpdateEmployeeRoleAsync(int updaterEmployeeId, [FromBody] UpdateEmployeeRoleDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the updater employee exists
            var updaterEmployee = await _employeeRepository.GetByIdDtoAsync(updaterEmployeeId, cancellationToken);
            if (updaterEmployee is null)
            {
                return NotFound(new { error = "Supervisor employee not found" });
            }

            // Check if the updater employee's access level is at least 7
            if (updaterEmployee.AccessLevel < updateEmployeeAccessLevel)
            {
                return BadRequest(new { error = "Updater employee does not have permission to update employee role" });
            }

            // Check if the updater has a higher access level than the employee being updated
            var employee = await _employeeRepository.GetByIdDtoAsync(dto.EmployeeID, cancellationToken);
            if (employee is null)
            {
                return NotFound(new { error = "Employee not found" });
            }
            if (updaterEmployee.AccessLevel <= employee.AccessLevel)
            {
                return BadRequest(new { error = "Updater employee does not have permission to update employee role" });
            }

            // Update the employee role
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
    [HttpPut("{updaterEmployeeId:int}/access-level")]
    public async Task<IActionResult> UpdateEmployeeAccessLevelAsync(int updaterEmployeeId, [FromBody] UpdateEmployeeAccessLevelDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the updater employee exists
            var updaterEmployee = await _employeeRepository.GetByIdDtoAsync(updaterEmployeeId, cancellationToken);
            if (updaterEmployee is null)
            {
                return NotFound(new { error = "Updater employee not found" });
            }

            // Check if the updater employee's access level is at least 7
            if (updaterEmployee.AccessLevel < updateEmployeeAccessLevel)
            {
                return BadRequest(new { error = "Updater employee does not have permission to update employee access level" });
            }

            // Check if the employee exists
            var employee = await _employeeRepository.GetByIdDtoAsync(dto.EmployeeID, cancellationToken);
            if (employee is null)
            {
                return NotFound(new { error = "Employee not found" });
            }
            if (updaterEmployee.AccessLevel <= employee.AccessLevel)
            {
                return BadRequest(new { error = "Updater employee does not have permission to update employee access level" });
            }
            
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
    [HttpDelete("{deleter-employeeId:int}/{employeeId:int}/delete-employee")]
    public async Task<IActionResult> DeleteAsync(int deleterEmployeeId, int employeeId, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the deleter employee exists
            var deleterEmployee = await _employeeRepository.GetByIdDtoAsync(deleterEmployeeId, cancellationToken);
            if (deleterEmployee is null)
            {
                return NotFound(new { error = "Deleter employee not found" });
            }

            // Check if the deleter employee's access level is at least 7
            if (deleterEmployee.AccessLevel < deleteEmployeeAccessLevel)
            {
                return BadRequest(new { error = "Deleter employee does not have permission to delete employees" });
            }
            
            // Check if the employee to be deleted exists
            var employee = await _employeeRepository.GetByIdDtoAsync(employeeId, cancellationToken);
            if (employee is null)
            {
                return NotFound(new { error = "Employee not found" });
            }

            // Delete the employee
            var deleted = await _employeeRepository.DeleteAsync(employeeId, cancellationToken);
            
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