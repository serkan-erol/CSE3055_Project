using Microsoft.AspNetCore.Mvc;
using Kismet.Entities.DTOs;
using Kismet.Repository.Interfaces;

namespace Kismet.API.Controllers;

// [Authorize] // Commented out - no authorization for this project
[ApiController]
[Route("api/[controller]")]
public class FabricController : ControllerBase
{
    private readonly IFabricRepository _fabricRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<FabricController> _logger;

    private const int minAccessLevel = 5;

    public FabricController(IFabricRepository fabricRepository, IEmployeeRepository employeeRepository, ILogger<FabricController> logger)
    {
        _fabricRepository = fabricRepository;
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all fabrics
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllFabrics(CancellationToken cancellationToken)
    {
        try
        {
            var fabrics = await _fabricRepository.GetAllFabricsAsync(cancellationToken);
            return Ok(fabrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fabrics");
            return StatusCode(500, "An error occurred while retrieving fabrics");
        }
    }

    /// <summary>
    /// Get fabric by ID
    /// </summary>
    [HttpGet("{fabricId}")]
    public async Task<IActionResult> GetFabricById(int fabricId, CancellationToken cancellationToken)
    {
        try
        {
            var fabric = await _fabricRepository.GetFabricByIdAsync(fabricId, cancellationToken);
            return Ok(fabric);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Fabric {FabricId} not found", fabricId);
            return NotFound(new { message = "Fabric not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fabric {FabricId}", fabricId);
            return StatusCode(500, "An error occurred while retrieving fabric");
        }
    }

    /// <summary>
    /// Create new fabric
    /// </summary>
    // [Authorize(Roles = "Employee")] // Commented out - no authorization
    [HttpPost("{employeeId:int}/create-fabric")]
    public async Task<IActionResult> CreateFabric(int employeeId, [FromBody] CreateFabricDto dto, CancellationToken cancellationToken)
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
                return BadRequest(new { error = "Employee does not have permission to create fabrics" });
            }

            var fabric = await _fabricRepository.CreateFabricAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetFabricById), new { fabricId = fabric.FabricID }, fabric);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating fabric");
            return StatusCode(500, "An error occurred while creating fabric");
        }
    }

    /// <summary>
    /// Update fabric stock
    /// </summary>
    // [Authorize(Roles = "Employee")] // Commented out - no authorization
    [HttpPatch("{fabricId}/stock")]
    public async Task<IActionResult> UpdateFabricStock(int fabricId, [FromBody] UpdateFabricStockDto dto, CancellationToken cancellationToken)
    {
        try
        {
            dto.FabricID = fabricId;
            var fabric = await _fabricRepository.UpdateFabricStockAsync(dto, cancellationToken);
            return Ok(fabric);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Fabric {FabricId} not found", fabricId);
            return NotFound(new { message = "Fabric not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating fabric stock for {FabricId}", fabricId);
            return StatusCode(500, "An error occurred while updating fabric stock");
        }
    }
}