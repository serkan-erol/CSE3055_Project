using Microsoft.AspNetCore.Mvc;
using Kismet.Entities.DTOs;
using Kismet.Repository.Interfaces;

namespace Kismet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BatchController : ControllerBase
{
    private readonly IBatchRepository _batchRepository;
    private readonly ILogger<BatchController> _logger;

    public BatchController(IBatchRepository batchRepository, ILogger<BatchController> logger)
    {
        _batchRepository = batchRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get batches by order ID
    /// Returns customer view for customers, employee view for employees
    /// </summary>
    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetBatchesByOrderId(int orderId, CancellationToken cancellationToken)
    {
        try
        {
            // Without authorization, just return employee view with all details
            // If you want to differentiate later, uncomment authorization
            var batches = await _batchRepository.GetBatchesByOrderIdForEmployeeAsync(orderId, cancellationToken);
            return Ok(batches);

            /* With authorization enabled:
            var userType = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userType == "Customer")
            {
                var batches = await _batchRepository.GetBatchesByOrderIdForCustomerAsync(orderId, cancellationToken);
                return Ok(batches);
            }
            else if (userType == "Employee")
            {
                var batches = await _batchRepository.GetBatchesByOrderIdForEmployeeAsync(orderId, cancellationToken);
                return Ok(batches);
            }

            return Forbid();
            */
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting batches for order {OrderId}", orderId);
            return StatusCode(500, "An error occurred while retrieving batches");
        }
    }

    /// <summary>
    /// Get batch by ID
    /// </summary>
    // [Authorize(Roles = "Employee")] // Commented out - no authorization
    [HttpGet("{batchId}")]
    public async Task<IActionResult> GetBatchById(int batchId, CancellationToken cancellationToken)
    {
        try
        {
            var batch = await _batchRepository.GetBatchByIdAsync(batchId, cancellationToken);
            return Ok(batch);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Batch {BatchId} not found", batchId);
            return NotFound(new { message = "Batch not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting batch {BatchId}", batchId);
            return StatusCode(500, "An error occurred while retrieving batch");
        }
    }

    /// <summary>
    /// Get unshipped batches for an order
    /// </summary>
    // [Authorize(Roles = "Employee")] // Commented out - no authorization
    [HttpGet("order/{orderId}/unshipped")]
    public async Task<IActionResult> GetUnshippedBatches(int orderId, CancellationToken cancellationToken)
    {
        try
        {
            var batches = await _batchRepository.GetUnshippedBatchesByOrderIdAsync(orderId, cancellationToken);
            return Ok(batches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unshipped batches for order {OrderId}", orderId);
            return StatusCode(500, "An error occurred while retrieving unshipped batches");
        }
    }

    /// <summary>
    /// Create batches for multiple fabrics
    /// </summary>
    [HttpPost("create-multiple-fabrics/{orderId}")]
    public async Task<IActionResult> CreateBatchesForMultipleFabrics(int orderId, [FromBody] CreateBatchesForMultipleFabricsDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _batchRepository.CreateBatchesForMultipleFabricsAsync(orderId, dto.FabricIDs, dto.Quantities, dto.QualityGrades, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating batches for multiple fabrics for order {OrderId}", orderId);
            return StatusCode(500, new { message = ex.Message });
        }
    }
}