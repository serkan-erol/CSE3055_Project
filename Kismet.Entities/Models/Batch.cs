using System;

namespace Kismet.Entities.Models;

public class Batch
{
    public int BatchId { get; set; }
    public int FabricId { get; set; }
    public int ShipmentId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime? ProductionDate { get; set; }
    public string? QualityGrade { get; set; }
}

