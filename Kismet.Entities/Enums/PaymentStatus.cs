namespace Kismet.Entities.Enums 
{
    public enum PaymentStatus
    {
        Unpaid = 0,
        Partial = 1,
        Paid = 2
    }

    public static class PaymentStatusExtensions
    {
        public static string GetDisplayName(this PaymentStatus paymentStatus)
        {
            return paymentStatus switch
            {
                PaymentStatus.Unpaid => "Unpaid",
                PaymentStatus.Partial => "Partial",
                PaymentStatus.Paid => "Paid",
                _ => "Unknown"
            };
        }
    }
}