using System;

namespace Kismet.Entities.Models;

public class Payment
{
    public int PaymentId { get; set; }
    public int TransactionId { get; set; }
    public int? BillingId { get; set; }
    public decimal PaymentAmount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
}

