using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Kismet.Entities.Enums;

namespace Kismet.Entities.DTOs;

/// <summary>
/// DTO for returning Payment data to API clients
/// </summary>
public class PaymentResponseDto
{
    public int PaymentID { get; set; }
    public int FTransactionID { get; set; }
    public decimal PaymentAmount { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public DateTimeOffset PaymentDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
}

/// <summary>
/// DTO for creating a new Payment
/// </summary>
public class CreatePaymentDto
{
    [Required]
    [JsonIgnore]
    public int FTransactionID { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "PaymentAmount must be greater than 0")]
    public decimal PaymentAmount { get; set; }

    [Required]
    [StringLength(8)]
    public string PaymentType { get; set; } = string.Empty;

    [StringLength(50)]
    public string? PaymentMethod { get; set; }

    [StringLength(100)]
    public string? ReferenceNumber { get; set; }
}