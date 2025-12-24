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
    private readonly ILogger<TreasuryController> _logger;

    public TreasuryController(
        ITreasuryRepository treasuryRepository,
        ILogger<TreasuryController> logger)
    {
        _treasuryRepository = treasuryRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all treasury entries (employees only)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllTreasuryEntries(CancellationToken cancellationToken)
    {
        try
        {
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
    [HttpGet("{treasuryId}")]
    public async Task<IActionResult> GetTreasuryEntryById(int treasuryId, CancellationToken cancellationToken)
    {
        try
        {
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
    [HttpGet("balance")]
    public async Task<IActionResult> GetCurrentBalance(CancellationToken cancellationToken)
    {
        try
        {
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
    [HttpGet("transaction/{fTransactionId}")]
    public async Task<IActionResult> GetTreasuryEntriesByTransactionId(int fTransactionId, CancellationToken cancellationToken)
    {
        try
        {
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