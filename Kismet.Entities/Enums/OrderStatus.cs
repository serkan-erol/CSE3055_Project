namespace Kismet.Entities.Enums 
{
    public enum OrderStatus
    {
        Pending = 0,
        Approved = 1,
        Shipped = 2,
        Delivered = 3,
        Cancelled = 4
    }

    public static class OrderStatusExtensions
    {
        public static string GetDisplayName(this OrderStatus orderStatus)
        {
            return orderStatus switch
            {
                OrderStatus.Pending => "Pending",
                OrderStatus.Approved => "Approved",
                OrderStatus.Shipped => "Shipped",
                OrderStatus.Delivered => "Delivered",
                OrderStatus.Cancelled => "Cancelled",
                _ => "Unknown"
            };
        }
    }
}