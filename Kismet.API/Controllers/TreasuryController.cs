using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Kismet.Entities.DTOs;
using Kismet.Repository.Interfaces;
using System.Security.Claims;

namespace Kismet.API.Controllers;

//[Authorize(Roles = "Employee")]
[ApiController]
[Route("api/[controller]")]
public class TreasuryController : ControllerBase
{
    private readonly ITreasuryRepository _treasuryRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<TreasuryController> _logger;

    private const int accountantAccessLevel = 4;
    private const int sysAdminAccessLevel = 9;

    public TreasuryController(
        ITreasuryRepository treasuryRepository,
        IEmployeeRepository employeeRepository,
        ILogger<TreasuryController> logger)
    {
        _treasuryRepository = treasuryRepository;
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all treasury entries (employees only)
    /// </summary>
    [HttpGet("{employeeId:int}")]
    public async Task<IActionResult> GetAllTreasuryEntries(int employeeId, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the employee exists
            var employee = await _employeeRepository.GetByIdDtoAsync(employeeId, cancellationToken);
            if (employee is null)
            {
                return NotFound(new { error = "Employee not found" });
            }

            // Check if the employee's access level is exactly 4 or it is 9+ (Accountant or Sys_Admin, CEO)
            if (employee.AccessLevel != accountantAccessLevel && employee.AccessLevel < sysAdminAccessLevel)
            {
                return BadRequest(new { error = "Employee does not have permission to get all treasury entries" });
            }

            var entries = await _treasuryRepository.GetAllTreasuryEntriesAsync(cancellationToken);
            return Ok(entries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting treasury entries");
            return StatusCode(500, "An error occurred while retrieving treasury entries");
        }
    }

    /// <summary>
    /// Get treasury entry by ID (employees only)
    /// </summary>
    [HttpGet("{employeeId:int}/{treasuryId}/get-by-id")]
    public async Task<IActionResult> GetTreasuryEntryById(int employeeId, int treasuryId, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the employee exists
            var employee = await _employeeRepository.GetByIdDtoAsync(employeeId, cancellationToken);
            if (employee is null)
            {
                return NotFound(new { error = "Employee not found" });
            }

            // Check if the employee's access level is exactly 4 (Accountant)
            if (employee.AccessLevel < accountantAccessLevel)
            {
                return BadRequest(new { error = "Employee does not have permission to get treasury entry by ID" });
            }

            var entry = await _treasuryRepository.GetTreasuryEntryByIdAsync(treasuryId, cancellationToken);
            return Ok(entry);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Treasury entry {TreasuryId} not found", treasuryId);
            return NotFound(new { message = "Treasury entry not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting treasury entry {TreasuryId}", treasuryId);
            return StatusCode(500, "An error occurred while retrieving treasury entry");
        }
    }

    /// <summary>
    /// Get current treasury balance (employees only)
    /// </summary>
    [HttpGet("{employeeId:int}/balance")]
    public async Task<IActionResult> GetCurrentBalance(int employeeId, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the employee exists
            var employee = await _employeeRepository.GetByIdDtoAsync(employeeId, cancellationToken);
            if (employee is null)
            {
                return NotFound(new { error = "Employee not found" });
            }

            // Check if the employee's access level is at least 4
            if (employee.AccessLevel < accountantAccessLevel)
            {
                return BadRequest(new { error = "Employee does not have permission to get current treasury balance" });
            }

            var balance = await _treasuryRepository.GetCurrentBalanceAsync(cancellationToken);
            return Ok(balance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting treasury balance");
            return StatusCode(500, "An error occurred while retrieving treasury balance");
        }
    }

    /// <summary>
    /// Get treasury entries by financial transaction ID (employees only)
    /// </summary>
    [HttpGet("{employeeId:int}/{fTransactionId}/get-by-transaction-id")]
    public async Task<IActionResult> GetTreasuryEntriesByTransactionId(int employeeId, int fTransactionId, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the employee exists
            var employee = await _employeeRepository.GetByIdDtoAsync(employeeId, cancellationToken);
            if (employee is null)
            {
                return NotFound(new { error = "Employee not found" });
            }

            // Check if the employee's access level is at least 4
            if (employee.AccessLevel < accountantAccessLevel)
            {
                return BadRequest(new { error = "Employee does not have permission to get treasury entries by transaction ID" });
            }

            var entries = await _treasuryRepository.GetTreasuryEntriesByTransactionIdAsync(fTransactionId, cancellationToken);
            return Ok(entries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting treasury entries for transaction {FTransactionId}", fTransactionId);
            return StatusCode(500, "An error occurred while retrieving treasury entries");
        }
    }
}