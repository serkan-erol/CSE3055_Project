using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Kismet.Entities.DTOs;


/// <summary>
/// DTO for returning Batch data to customers
/// </summary>
public class BatchResponseToCustomerDto
{
    public string BatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal BatchPrice { get; set; }
    public DateTime? ProductionDate { get; set; }
    public string? QualityGrade { get; set; }
}

/// <summary>
/// DTO for returning Batch data to employees
/// </summary>
public class BatchResponseToEmployeeDto
{
    public int BatchID { get; set; }
    public int OrderID { get; set; }
    public int? ShipmentID { get; set; }
    public int FabricID { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal BatchPrice { get; set; }
    public DateTime? ProductionDate { get; set; }
    public string? QualityGrade { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating batches (stored procedure)
/// </summary>
/// <summary>
/// DTO for creating batches (stored procedure)
/// </summary>
public class CreateBatchesDto
{
    public int OrderID { get; set; }
    public int FabricID { get; set; }
    public int TotalFabricUnits { get; set; }
    public string? QualityGrade { get; set; }
}

/// <summary>
/// DTO for batch creation response
/// </summary>
public class CreateBatchesResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
