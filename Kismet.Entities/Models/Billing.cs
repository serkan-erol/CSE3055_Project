using System;

namespace Kismet.Entities.Models;

public class Billing
{
    public int BillingId { get; set; }
    public int CustomerId { get; set; }
    public int OrderId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal TotalDue { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal RemainingBalance => TotalDue - TotalPaid;
    public string? PaymentTerms { get; set; }
    public DateTime BillingDate { get; set; }
    public string? Status { get; set; }
}

