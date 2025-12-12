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
    /// Update user contact/name info (works for Customer or Employee)
    /// </summary>
    [HttpPut("{id:int}/name")]
    public async Task<IActionResult> UpdateUserNameAsync(int id, [FromBody] UpdateUserNameDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dto.UserID = id;

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
    [HttpPut("{id:int}/email")]
    public async Task<IActionResult> UpdateUserEmailAsync(int id, [FromBody] UpdateUserEmailDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dto.UserID = id;

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
    [HttpPut("{id:int}/phone")]
    public async Task<IActionResult> UpdateUserPhoneAsync(int id, [FromBody] UpdateUserPhoneDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dto.UserID = id;

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
    [HttpPut("{id:int}/password")]
    public async Task<IActionResult> UpdateUserPasswordAsync(int id, [FromBody] UpdateUserPasswordDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dto.UserID = id;

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