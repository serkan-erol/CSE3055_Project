using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Kismet.Entities.Enums;

namespace Kismet.Entities.DTOs;

/// <summary>
/// DTO for returning FT data to API clients
/// </summary>
public class FTResponseToEmployeeDto
{
    public int FTransactionID { get; set; }
    public int CustomerID { get; set; }
    public int BillingID { get; set; }
    public int OrderID { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal RemainingBalance { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTimeOffset? LastUpdatedAt { get; set; }
}

/// <summary>
/// DTO for returning FT data to API clients
/// </summary>
public class FTResponseToCustomerDto
{
    public int FTransactionID { get; set; }
    public int CustomerID { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal RemainingBalance { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTimeOffset? LastUpdatedAt { get; set; }
}

/// <summary>
/// DTO for returning PaymentStatus
/// </summary>
public class FTPaymentStatusResponseDto
{
    public PaymentStatus PaymentStatus { get; set; }
}

/// <summary>
/// DTO for creating a new FT
/// </summary>
public class CreateFTDto
{
    [Required]
    [JsonIgnore]
    public int CustomerID { get; set; }

    // After some checks, we may assign this FT to an existing Billing entry OR
    // We may create a new Billing entry for this FT
    // Then, we can assign the associated BillingID to the new FT
    [Required]
    [JsonIgnore]
    public int BillingID { get; set; }

    [Required]
    [JsonIgnore]
    public int OrderID { get; set; }

    [Required]
    [StringLength(8)]
    public string TransactionType { get; set; } = string.Empty;

    [Required]
    //aaa [Range(0, double.MaxValue, ErrorMessage = "TotalAmount must be greater than 0")]
    public decimal TotalAmount { get; set; }

    //aaa [Range(0, double.MaxValue, ErrorMessage = "TotalPaid must be greater than 0")]
    public decimal? TotalPaid { get; set; } = 0;

    [StringLength(255)]
    public string? Description { get; set; }

    public DateTime? TransactionDate { get; set; } = DateTime.UtcNow.AddMonths(6);
}

/// <summary>
/// DTO for updating the total paid of a FT
/// </summary>
public class UpdateFTTotalPaidDto   
{
    [Required]
    [JsonIgnore]
    public int FTransactionID { get; set; }

    [Required]
    //aaa [Range(0, double.MaxValue, ErrorMessage = "TotalPaid must be greater than 0")]
    public decimal TotalPaid { get; set; }
}

/// <summary>
/// DTO for updating the Description of a FT
/// </summary>
public class UpdateFTDescriptionDto
{
    [Required]
    [JsonIgnore]
    public int FTransactionID { get; set; }

    [Required]
    [StringLength(255)]
    public string Description { get; set; } = string.Empty;
}