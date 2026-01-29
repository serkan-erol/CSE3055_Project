using System.ComponentModel.DataAnnotations;

namespace Kismet.Entities.DTOs;

/// <summary>
/// DTO for returning saved payment method data
/// </summary>
public class SavedPaymentMethodResponseDto
{
    public int SPMID { get; set; }
    public int CustomerID { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public string CardType { get; set; } = string.Empty;
    public DateOnly CardExpirationDate { get; set; }
    public DateOnly? RecordExpirationDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastUpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new saved payment method
/// </summary>
public class CreateSavedPaymentMethodDto
{
    [Required]
    public int CustomerID { get; set; }
    
    [Required]
    [StringLength(255)]
    public string CardNumber { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string CardType { get; set; } = string.Empty; // 'Debit' or 'Credit'
    
    [Required]
    public DateOnly CardExpirationDate { get; set; }
    
    public DateOnly? RecordExpirationDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(5);
}

/// <summary>
/// DTO for updating an existing saved payment method
/// </summary>
public class UpdateSavedPaymentMethodDto
{
    [StringLength(255)]
    public string? CardNumber { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string? CardType { get; set; } = string.Empty; // 'Debit' or 'Credit'
    
    public DateOnly? CardExpirationDate { get; set; }
    
    public DateOnly? RecordExpirationDate { get; set; }
}

/// <summary>
/// DTO for returning saved bank information data
/// </summary>
public class SavedBankInformationResponseDto
{
    public int SBIID { get; set; }
    public int CustomerID { get; set; }
    public string? BankName { get; set; }
    public string? AccountNo { get; set; }
    public string? IBAN { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastUpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating new saved bank information
/// </summary>
public class CreateSavedBankInformationDto
{
    [Required]
    public int CustomerID { get; set; }
    
    [StringLength(50)]
    public string? BankName { get; set; }
    
    [StringLength(50)]
    public string? AccountNo { get; set; }
    
    [StringLength(50)]
    public string? IBAN { get; set; }
}

/// <summary>
/// DTO for updating existing saved bank information
/// </summary>
public class UpdateSavedBankInformationDto
{
    [StringLength(50)]
    public string? BankName { get; set; }
    
    [StringLength(50)]
    public string? AccountNo { get; set; }
    
    [StringLength(50)]
    public string? IBAN { get; set; }
}
