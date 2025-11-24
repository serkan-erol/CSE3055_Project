using System;

namespace Kismet.Entities.Models;

public class Treasury
{
    public int TreasuryId { get; set; }
    public int TransactionId { get; set; }
    public DateTime EntryDate { get; set; }
    public decimal Amount { get; set; }
    public decimal? BalanceAfter { get; set; }
    public string? Description { get; set; }
}

