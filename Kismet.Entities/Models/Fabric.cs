namespace Kismet.Entities.Models;

public class Fabric
{
    public int FabricId { get; set; }
    public string FabricType { get; set; } = string.Empty;
    public string? Composition { get; set; }
    public string? Color { get; set; }
    public decimal? WeightPerUnit { get; set; }
    public int StockQuantity { get; set; }
    public string? Unit { get; set; }
}

