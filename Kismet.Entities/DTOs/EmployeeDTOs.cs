using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Kismet.Entities.DTOs;

/// <summary>
/// DTO for returning Customer data to API clients
/// Combines data from Customer and User tables
/// </summary>
public class EmployeeResponseDto
{
    // From Employee table
    public int EmployeeID { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeRole { get; set; } = string.Empty;
    public int AccessLevel { get; set; }
    
    // From User table
    public string UserName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastUpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new Customer
/// Contains only the data needed to create a Employee
/// </summary>
public class CreateEmployeeDto
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

    [Required]
    [StringLength(50)]
    public string EmployeeRole { get; set; } = string.Empty;

    [Required]
    public int AccessLevel { get; set; }
}

/// <summary>
/// DTO for updating EmployeeRole
/// </summary>
public class UpdateEmployeeRoleDto
{
    [Required]
    public int EmployeeID { get; set; }

    [Required]
    [StringLength(50)]
    public string EmployeeRole { get; set; } = string.Empty;

}

/// <summary>
/// DTO for updating AccessLevel of an Employee
/// </summary>
public class UpdateEmployeeAccessLevelDto
{
    [Required]
    public int EmployeeID { get; set; }

    [Required]
    [Range(1, 10)]
    public int AccessLevel { get; set; }
}