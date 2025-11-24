namespace Kismet.Entities.Models;

public class Customer
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerType { get; set; }
    public string? ReliabilityStatus { get; set; }
    public string? PaymentType { get; set; }
    public string? ContactInfo { get; set; }
}

