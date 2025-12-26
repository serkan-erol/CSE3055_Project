using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Kismet.Entities.DTOs;

// ============ FABRIC DTOs ============

/// <summary>
/// DTO for returning Fabric data
/// </summary>
public class FabricResponseDto
{
    public int FabricID { get; set; }
    public string FabricType { get; set; } = string.Empty;
    public string? Composition { get; set; }
    public string? Color { get; set; }
    public decimal? WeightPerUnit { get; set; }
    public int StockQuantity { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// DTO for creating a new Fabric
/// </summary>
public class CreateFabricDto
{
    [Required]
    public string FabricType { get; set; } = string.Empty;

    public string? Composition { get; set; }
    public string? Color { get; set; }
    public decimal? WeightPerUnit { get; set; }

    [Required]
    public int StockQuantity { get; set; }

    [Required]
    public decimal UnitPrice { get; set; }

    public string? Description { get; set; }
}

/// <summary>
/// DTO for updating Fabric stock
/// </summary>
public class UpdateFabricStockDto
{
    [Required]
    [JsonIgnore]
    public int FabricID { get; set; }

    [Required]
    public int QuantityChange { get; set; } // Positive to add, negative to reduce
}
