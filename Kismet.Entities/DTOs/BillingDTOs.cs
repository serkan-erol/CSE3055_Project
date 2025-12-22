using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Kismet.Entities.Enums;

namespace Kismet.Entities.DTOs;

/// <summary>
/// DTO for returning Billing data to API clients
/// </summary>
public class BillingResponseToEmployeeDto
{
    public int BillingID { get; set; }
    public int CustomerID { get; set; }
    public string BillingType { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal TotalDue { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal RemainingBalance { get; set; }
    public string? PaymentTerms { get; set; }
    public DateTime BillingDate { get; set; }
    public PaymentStatus BillingStatus { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastUpdatedAt { get; set; }
}

/// <summary>
/// DTO for returning Billing data to API clients
/// </summary>
public class BillingResponseToCustomerDto
{
    public string BillingType { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal TotalDue { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal RemainingBalance { get; set; }
    public string? PaymentTerms { get; set; }
    public DateTime BillingDate { get; set; }
    public DateTimeOffset? LastUpdatedAt { get; set; }
}

/// <summary>
/// DTO for returning BillingStatus
/// </summary>
public class BillingStatusResponseDto
{
    public PaymentStatus BillingStatus { get; set; }
}

/// <summary>
/// DTO for creating a new Billing
/// </summary>
public class CreateBillingDto
{
    [Required]
    [JsonIgnore]
    public int CustomerID { get; set; }

    [Required]
    [StringLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "TotalDue must be greater than 0")]
    public decimal TotalDue { get; set; }

    [StringLength(255)]
    public string? PaymentTerms { get; set; }

    //aaa [DataType(DataType.Date)]
    public DateTime? BillingDate { get; set; }
}

/// <summary>
/// DTO for creating a new Billing
/// </summary>
public class UpdateBillingTypeDto   
{
    [JsonIgnore]
    public int BillingID { get; set; }

    // 'Purchase' for the customer buying from us and 'Supply' for us buying from the customer
    [Required]
    [StringLength(8)]
    public string BillingType { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating Billing
/// </summary>
public class UpdateBillingDto
{
    // Set by controller from route; not accepted from request body
    // So we can prevent people from changing IDs in the DB
    [JsonIgnore]
    public int BillingID { get; set; }

    //aaa [Required]
    //aaa [Range(0, double.MaxValue, ErrorMessage = "TotalDue must be greater than 0")]
    public decimal? TotalDue { get; set; }

    //aaa [Required]
    //aaa [Range(0, double.MaxValue, ErrorMessage = "TotalPaid must be greater than 0")]
    public decimal? TotalPaid { get; set; }
}

/// <summary>
/// DTO for updating BillingStatus
/// </summary>
public class UpdateBillingPaymentTermsDto
{
    [JsonIgnore]
    public int BillingID { get; set; }

    [StringLength(255)]
    public string? PaymentTerms { get; set; }
}

/// <summary>
/// DTO for updating BillingDate
/// </summary>
public class UpdateBillingDateDto
{
    [JsonIgnore]
    public int BillingID { get; set; }

    [Required]
    //aaa [DataType(DataType.Date)]
    public DateTime BillingDate { get; set; }
}