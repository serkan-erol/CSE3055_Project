using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Kismet.Entities.DTOs;

/// <summary>
/// DTO for returning Session data to API clients
/// </summary>
public class SessionResponseDto
{
    public int SessionID { get; set; }
    public int UserID { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTimeOffset ATExpiresAt { get; set; }
    public DateTimeOffset RTExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastUpdatedAt { get; set; }
}

/// <summary>
/// DTO for returning Session data to API clients for login
/// Can be used to update AccessToken and RefreshToken
/// </summary>
public class TokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTimeOffset ATExpiresAt { get; set; }
    public DateTimeOffset RTExpiresAt { get; set; }
}

/// <summary>
/// DTO for Login
/// Can be used for login and password verification
/// </summary>
public class LoginDto
{
    [Required]
    [StringLength(100)]
    public string ContactEmail { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO for creating a new Session
/// </summary>
public class CreateSessionDto
{
    [Required]
    [JsonIgnore]
    public int UserID { get; set; }

    [StringLength(255)]
    public string? AccessToken { get; set; }

    [Required]
    [StringLength(255)]
    public string RefreshToken { get; set; } = string.Empty;

    [Required]
    public DateTimeOffset? ATExpiresAt { get; set; }

    [Required]
    public DateTimeOffset RTExpiresAt { get; set; }
}