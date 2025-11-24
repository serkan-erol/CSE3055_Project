using System;

namespace Kismet.Entities.Models;

public class Shipment
{
    public int ShipmentId { get; set; }
    public int OrderId { get; set; }
    public string? CustomsDocRef { get; set; }
    public string? ShipmentStatus { get; set; }
    public DateTime? ShipmentDate { get; set; }
    public string? OriginCountry { get; set; }
    public string? DestinationCountry { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
}

