using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Kismet.Entities.DTOs;

// There is no DTO or repository for User creation. 
// User creation is handled separately depending on the UserType.
// Customer and Employee creation triggers a User entry first,
// then creates the corresponding Customer / Employee entry.

/// <summary>
/// DTO for returning User data to API clients
/// </summary>
public class UserResponseDto
{
    public int UserID { get; set; }
    public string UserType { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastUpdatedAt { get; set; }
}

/// <summary>
/// DTO for returning User data to API clients for login
/// Can be used for password verification
/// </summary>
public class GetUserByEmailResponseDto
{
    public int UserID { get; set; }
    public string UserType { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating UserName
/// </summary>
public class UpdateUserNameDto
{
    // Set by controller from route; not accepted from request body
    [JsonIgnore]
    public int UserID { get; set; }

    [StringLength(100)]
    public string UserName { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating User's email
/// </summary>
public class UpdateUserEmailDto
{
    // Set by controller from route; not accepted from request body
    [JsonIgnore]
    public int UserID { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string ContactEmail { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating User's Phone number
/// </summary>
public class UpdateUserPhoneDto
{
    // Set by controller from route; not accepted from request body
    [JsonIgnore]
    public int UserID { get; set; }

    [StringLength(100)]
    public string ContactPhone { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating User's Password
/// Can be used for password verification
/// </summary>
public class UpdateUserPasswordDto
{
    // Set by controller from route; not accepted from request body
    [JsonIgnore]
    public int UserID { get; set; }

    [Required]
    [StringLength(100)]
    public string ContactEmail { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string OldPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string NewPassword { get; set; } = string.Empty;
}