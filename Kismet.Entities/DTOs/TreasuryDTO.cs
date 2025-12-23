using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Kismet.Entities.DTOs;

/// <summary>
/// DTO for returning Treasury data to employees
/// </summary>
public class TreasuryResponseDto
{
    public int TreasuryID { get; set; }
    public int FTransactionID { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset EntryDate { get; set; }
    public DateTimeOffset? LastUpdatedAt { get; set; }
}

/// <summary>
/// DTO for manually adding money to treasury (employees only)
/// </summary>
public class AddMoneyToTreasuryDto
{
    [Required]
    public decimal Amount { get; set; }

    [Required]
    [JsonIgnore]
    public int EmployeeID { get; set; }

    public string? Description { get; set; }
}

/// <summary>
/// DTO for getting current treasury balance
/// </summary>
public class TreasuryBalanceDto
{
    public decimal CurrentBalance { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
    public int TotalEntries { get; set; }
}