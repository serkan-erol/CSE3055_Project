using System;
using System.ComponentModel.DataAnnotations;

namespace Kismet.Entities.Models;

public class FinancialTransaction
{
    [Key]
    public int FinancialTransactionId { get; set; }
    public int CustomerId { get; set; }
    public int OrderId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? PaymentStatus { get; set; }
    public string? Description { get; set; }
}

