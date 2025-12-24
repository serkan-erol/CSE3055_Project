using Kismet.Entities.DTOs;
using Kismet.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kismet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Get user by ID (works for Customer or Employee)
    /// </summary>
    [HttpGet("{userId:int}")]
    public async Task<IActionResult> GetByIdAsync(int userId, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdDtoAsync(userId, cancellationToken);
            
            if (user is null)
            {
                return NotFound(new { error = "User not found" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update user contact/name info (works for Customer or Employee)
    /// </summary>
    [HttpPut("{userId:int}/name")]
    public async Task<IActionResult> UpdateUserNameAsync(int userId, [FromBody] UpdateUserNameDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dto.UserID = userId;

            var updated = await _userRepository.UpdateUserNameAsync(dto, cancellationToken);

            if (updated is null)
            {
                return NotFound(new { error = "User not found" });
            }

            return Ok(new { message = "User name updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update user email info (works for Customer or Employee)
    /// </summary>
    [HttpPut("{userId:int}/email")]
    public async Task<IActionResult> UpdateUserEmailAsync(int userId, [FromBody] UpdateUserEmailDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dto.UserID = userId;

            var updated = await _userRepository.UpdateUserEmailAsync(dto, cancellationToken);

            if (updated is null)
            {
                return NotFound(new { error = "User not found" });
            }

            return Ok(new { message = "User email updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update user phone info (works for Customer or Employee)
    /// </summary>
    [HttpPut("{userId:int}/phone")]
    public async Task<IActionResult> UpdateUserPhoneAsync(int userId, [FromBody] UpdateUserPhoneDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dto.UserID = userId;

            var updated = await _userRepository.UpdateUserPhoneAsync(dto, cancellationToken);

            if (updated is null)
            {
                return NotFound(new { error = "User not found" });
            }

            return Ok(new { message = "User phone updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update user password (works for Customer or Employee)
    /// </summary>
    [HttpPut("{userId:int}/password")]
    public async Task<IActionResult> UpdateUserPasswordAsync(int userId, [FromBody] UpdateUserPasswordDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Set the userID in the DTO from the route parameter
            dto.UserID = userId;

            var updated = await _userRepository.UpdateUserPasswordAsync(dto, cancellationToken);

            if (updated is null)
            {
                return NotFound(new { error = "User not found" });
            }

            return Ok(new { message = "Password updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}