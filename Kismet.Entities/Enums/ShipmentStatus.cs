namespace Kismet.Entities.Enums 
{
    public enum ShipmentStatus
    {
        Pending = 0,
        InTransit = 1,
        Delivered = 2,
        Failed = 3
    }
    public static class ShipmentStatusExtensions
    {
        public static string GetDisplayName(this ShipmentStatus shipmentStatus)
        {
            return shipmentStatus switch
            {
                ShipmentStatus.Pending => "Pending",
                ShipmentStatus.InTransit => "In Transit",
                ShipmentStatus.Delivered => "Delivered",
                ShipmentStatus.Failed => "Failed",
                _ => "Unknown"
            };
        }
    }
}