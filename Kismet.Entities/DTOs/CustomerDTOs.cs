using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Kismet.Entities.DTOs;

/// <summary>
/// DTO for returning Customer data to API clients
/// Combines data from Customer and User tables
/// </summary>
public class CustomerResponseDto
{
    // From Customer table
    public int CustomerID { get; set; }
    public string CustomerNumber { get; set; } = string.Empty;
    public string? CustomerType { get; set; }
    public bool ReliabilityStatus { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    
    // From User table
    public string UserName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastUpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new Customer
/// Contains only the data needed to create a Customer
/// </summary>
public class CreateCustomerDto
{
    [Required]
    [StringLength(100)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string ContactEmail { get; set; } = string.Empty;

    [StringLength(20)]
    public string? ContactPhone { get; set; }

    [Required]
    [StringLength(255)]
    public string Password { get; set; } = string.Empty;

    [StringLength(50)]
    public string? CustomerType { get; set; }

    public bool ReliabilityStatus { get; set; } = true; // Default to reliable

    [StringLength(50)]
    public string? City { get; set; }

    [StringLength(50)]
    public string? Country { get; set; }
}

/// <summary>
/// DTO for updating CustomerType
/// </summary>
public class UpdateCustomerTypeDto
{
    // Set by controller from route; not accepted from request body
    [JsonIgnore]
    public int CustomerID { get; set; }

    [StringLength(50)]
    public string CustomerType { get; set; } = string.Empty;

}

/// <summary>
/// DTO for updating Customer's reliability information
/// </summary>
public class UpdateCustomerReliabilityDto
{
    // Set by controller from route; not accepted from request body
    [JsonIgnore]
    public int CustomerID { get; set; }

    public bool ReliabilityStatus { get; set; }
}

/// <summary>
/// DTO for updating Customer's city and country
/// </summary>
public class UpdateCustomerCityAndCountryDto
{
    [JsonIgnore]
    public int CustomerID { get; set; }

    [StringLength(50)]
    public string? City { get; set; }

    [StringLength(50)]
    public string? Country { get; set; }
}