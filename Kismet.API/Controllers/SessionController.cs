using Kismet.Core.Helpers;
using Kismet.Entities.DTOs;
using Kismet.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kismet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly JwtHelper _jwtHelper;

    public SessionController(ISessionRepository sessionRepository, 
                             IUserRepository userRepository,
                             ICustomerRepository customerRepository,
                             IEmployeeRepository employeeRepository,
                             JwtHelper jwtHelper)
    {
        _sessionRepository = sessionRepository;
        _userRepository = userRepository;
        _customerRepository = customerRepository;
        _employeeRepository = employeeRepository;
        _jwtHelper = jwtHelper;
    }

    /// <summary>
    /// Get session info by ID
    /// </summary>
    [HttpGet("{sessionId:int}")]
    public async Task<IActionResult> GetByIdAsync(int sessionId, CancellationToken cancellationToken)
    {
        try
        {
            var session = await _sessionRepository.GetByIdDtoAsync(sessionId, cancellationToken);
            if (session is null)
            {
                return NotFound(new { error = "Session not found" });
            }
            return Ok(session);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Failed to get session", exception = ex.Message });
        }
    }

    /// <summary>
    /// Get session info by user ID
    /// </summary>
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        try
        {
            var session = await _sessionRepository.GetSessionInfoByUserIdDtoAsync(userId, cancellationToken);
            if (session is null)
            {
                return NotFound(new { error = "Session not found" });
            }
            return Ok(session);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Failed to get session", exception = ex.Message });
        }
    }

    /// <summary>
    /// Login (Create or Update a session)
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        try
        {
            // Get the user by email
            var user = await _userRepository.GetByEmailAsync(dto.ContactEmail, cancellationToken);
            if (user is null)
            {
                return NotFound(new { error = "User not found" });
            }

            // Verify the password
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return BadRequest(new { error = "Invalid password" });
            }

            // Get the session by user ID
            var session = await _sessionRepository.GetSessionInfoByUserIdDtoAsync(user.UserID, cancellationToken);

            // If the session does not exist, create a new session
            if (session is null)
            {
                // Repository generates JWT tokens internally, but DTO requires these fields
                // so we provide default values that will be ignored
                var newSessionDto = new CreateSessionDto 
                    { 
                        UserID = user.UserID, 
                        Email = user.ContactEmail,
                        RefreshToken = string.Empty, // Will be ignored, repository generates it
                        RTExpiresAt = DateTimeOffset.UtcNow // Will be ignored, repository generates it
                    };

                // Create the new session (repository will generate JWT tokens)
                var newSession = await _sessionRepository.CreateAsync(newSessionDto, cancellationToken);
                
                // Check if the session was created successfully
                if (newSession is null)
                {
                    return BadRequest(new { error = "Failed to create session" });
                }

                // Set HTTP-only cookies with tokens
                SetTokenCookies(newSession.AccessToken, newSession.RefreshToken, newSession.ATExpiresAt, newSession.RTExpiresAt);

                return Ok(new { message = "Login successful", userId = user.UserID, sessionId = newSession.SessionID });  
            }

            // If the session exists but the RefreshToken is invalid or expired, update both the AccessToken and the RefreshToken
            if (session.RefreshToken is null || session.RTExpiresAt < DateTimeOffset.UtcNow)
            {
                // Generate new refresh token
                var newRefreshToken = _jwtHelper.GenerateRefreshToken();
                var refreshTokenExpiration = _jwtHelper.GetRefreshTokenExpiration();

                // Update the RefreshToken
                var updatedRefreshToken = await _sessionRepository.UpdateRefreshTokenAsync(new UpdateTokensDto 
                { 
                    SessionID = session.SessionID, 
                    RefreshToken = newRefreshToken, 
                    RTExpiresAt = refreshTokenExpiration
                }, cancellationToken);

                // Check if the refresh token was updated successfully
                if (updatedRefreshToken is null)
                {
                    return BadRequest(new { error = "Failed to update refresh token" });
                }

                // Generate new JWT access token based on user type
                string newAccessToken;
                if (user.UserType == "Customer")
                {
                    var customer = await _customerRepository.GetByIdDtoAsync(user.UserID, cancellationToken);
                    if (customer is null)
                    {
                        return BadRequest(new { error = "Customer not found" });
                    }
                    newAccessToken = _jwtHelper.GenerateCustomerAccessToken(user.ContactEmail, customer.CustomerID);
                }
                else if (user.UserType == "Employee")
                {
                    var employee = await _employeeRepository.GetByIdDtoAsync(user.UserID, cancellationToken);
                    if (employee is null)
                    {
                        return BadRequest(new { error = "Employee not found" });
                    }
                    newAccessToken = _jwtHelper.GenerateEmployeeAccessToken(user.ContactEmail, employee.EmployeeID, employee.EmployeeRole, employee.AccessLevel);
                }
                else
                {
                    return BadRequest(new { error = "Invalid user type" });
                }
                var accessTokenExpiration = _jwtHelper.GetAccessTokenExpiration();

                // Update the AccessToken
                var updatedAccessToken = await _sessionRepository.UpdateAccessTokenAsync(new UpdateTokensDto 
                { 
                    SessionID = session.SessionID, 
                    AccessToken = newAccessToken, 
                    ATExpiresAt = accessTokenExpiration
                }, cancellationToken);

                // Check if the access token was updated successfully
                if (updatedAccessToken is null)
                {
                    return BadRequest(new { error = "Failed to update access token" });
                }

                var updatedSessionResponse = await _sessionRepository.GetSessionInfoByUserIdDtoAsync(session.UserID, cancellationToken);

                // Check if the session response was retrieved successfully
                if (updatedSessionResponse is null)
                {
                    return BadRequest(new { error = "Failed to get session response" });
                }

                // Set HTTP-only cookies with tokens
                SetTokenCookies(updatedSessionResponse.AccessToken, updatedSessionResponse.RefreshToken, updatedSessionResponse.ATExpiresAt, updatedSessionResponse.RTExpiresAt);

                return Ok(new { message = "Login successful", userId = user.UserID, sessionId = updatedSessionResponse.SessionID });
            }

            // If the session exists but the AccessToken is invalid or expired, update the AccessToken
            if (session.AccessToken is null || session.ATExpiresAt < DateTimeOffset.UtcNow)
            {
                // Generate new JWT access token based on user type
                string newAccessToken;
                if (user.UserType == "Customer")
                {
                    var customer = await _customerRepository.GetByIdDtoAsync(user.UserID, cancellationToken);
                    if (customer is null)
                    {
                        return BadRequest(new { error = "Customer not found" });
                    }
                    newAccessToken = _jwtHelper.GenerateCustomerAccessToken(user.ContactEmail, customer.CustomerID);
                }
                else if (user.UserType == "Employee")
                {
                    var employee = await _employeeRepository.GetByIdDtoAsync(user.UserID, cancellationToken);
                    if (employee is null)
                    {
                        return BadRequest(new { error = "Employee not found" });
                    }
                    newAccessToken = _jwtHelper.GenerateEmployeeAccessToken(user.ContactEmail, employee.EmployeeID, employee.EmployeeRole, employee.AccessLevel);
                }
                else
                {
                    return BadRequest(new { error = "Invalid user type" });
                }
                var accessTokenExpiration = _jwtHelper.GetAccessTokenExpiration();

                var updatedAccessToken = await _sessionRepository.UpdateAccessTokenAsync(
                    new UpdateTokensDto 
                    { 
                        SessionID = session.SessionID, 
                        AccessToken = newAccessToken, 
                        ATExpiresAt = accessTokenExpiration
                    }, cancellationToken);

                // Check if the access token was updated successfully
                if (updatedAccessToken is null)
                {
                    return BadRequest(new { error = "Failed to update access token" });
                }
                var updatedSessionResponse = await _sessionRepository.GetSessionInfoByUserIdDtoAsync(session.UserID, cancellationToken);

                // Check if the session response was retrieved successfully
                if (updatedSessionResponse is null)
                {
                    return BadRequest(new { error = "Failed to get session response" });
                }

                // Set HTTP-only cookies with tokens
                SetTokenCookies(updatedSessionResponse.AccessToken, updatedSessionResponse.RefreshToken, updatedSessionResponse.ATExpiresAt, updatedSessionResponse.RTExpiresAt);

                return Ok(new { message = "Login successful", userId = user.UserID, sessionId = updatedSessionResponse.SessionID });
            }

            // If the session exists and is valid, set cookies and return success
            SetTokenCookies(session.AccessToken, session.RefreshToken, session.ATExpiresAt, session.RTExpiresAt);
            return Ok(new { message = "Login successful", userId = user.UserID, sessionId = session.SessionID });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Failed to login", exception = ex.Message });
        }
    }

    /// <summary>
    /// Logout (Expire the AccessToken)
    /// </summary>
    [HttpPost("logout/{userId:int}")]
    public async Task<IActionResult> LogoutAsync(int userId, CancellationToken cancellationToken)
    {
        try
        {

            // Get the session by user ID and session ID
            var session = await _sessionRepository.GetSessionInfoByUserIdDtoAsync(userId, cancellationToken);

            // Check if the session exists
            if (session is null)
            {
                return NotFound(new { error = "Session not found" });
            }

            // Expire the AccessToken
            var updatedAccessTokenExpiration = await _sessionRepository.ExpireAccessTokenAsync(
                new UpdateTokensDto 
                { 
                SessionID = session.SessionID, 
                ATExpiresAt = DateTimeOffset.UtcNow 
                }, cancellationToken);
            
            // Clear cookies on logout
            ClearTokenCookies();
            
            return Ok(new { message = "Logout successful" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Failed to logout", exception = ex.Message });
        }
    }

    /// <summary>
    /// Get current user info from the access token in cookie
    /// </summary>
    [HttpGet("current-user")]
    public async Task<IActionResult> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Get token from Authorization header (set by CookieTokenMiddleware)
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { error = "No valid token found" });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            
            // Extract user info from token
            var principal = _jwtHelper.GetPrincipalFromToken(token);
            var userTypeClaim = principal.FindFirst("UserType");
            var userType = userTypeClaim?.Value ?? "Customer";
            
            int userId;
            if (userType == "Customer")
            {
                var customerIdClaim = principal.FindFirst("CustomerID");
                if (customerIdClaim is null || !int.TryParse(customerIdClaim.Value, out userId))
                {
                    return Unauthorized(new { error = "Invalid token claims - CustomerID not found" });
                }
            }
            else if (userType == "Employee")
            {
                var employeeIdClaim = principal.FindFirst("EmployeeID");
                if (employeeIdClaim is null || !int.TryParse(employeeIdClaim.Value, out userId))
                {
                    return Unauthorized(new { error = "Invalid token claims - EmployeeID not found" });
                }
            }
            else
            {
                // Fallback to UserId for backward compatibility
                var userIdClaim = principal.FindFirst("UserId");
                if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out userId))
                {
                    return Unauthorized(new { error = "Invalid token claims" });
                }
            }

            // Get session to also return sessionId
            var session = await _sessionRepository.GetSessionInfoByUserIdDtoAsync(userId, cancellationToken);
            
            var response = new { 
                userId = userId,
                userType = userType,
                sessionId = session?.SessionID 
            };

            // Add role and accessLevel for employees
            if (userType == "Employee")
            {
                var roleClaim = principal.FindFirst("Role");
                var accessLevelClaim = principal.FindFirst("AccessLevel");
                return Ok(new { 
                    userId = userId,
                    userType = userType,
                    sessionId = session?.SessionID,
                    role = roleClaim?.Value,
                    accessLevel = accessLevelClaim != null && int.TryParse(accessLevelClaim.Value, out var accessLevel) ? accessLevel : (int?)null
                });
            }
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { error = "Failed to get current user", exception = ex.Message });
        }
    }

    /// <summary>
    /// Check if a session is valid
    /// </summary>
    [HttpGet("is-valid/{userId:int}")]
    public async Task<IActionResult> IsValidAsync(int userId, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the user exists
            var user = await _userRepository.GetByIdDtoAsync(userId, cancellationToken);
            if (user is null)
            {
                return NotFound(new { error = "User not found" });
            }

            // Check if the session exists
            var session = await _sessionRepository.GetSessionInfoByUserIdDtoAsync(userId, cancellationToken);
            if (session is null)
            {
                return NotFound(new { error = "Session not found" });
            }

            // Check if the session is valid
            var isValid = await _sessionRepository.IsSessionValidAsync(userId, cancellationToken);

            // Return the validity of the session
            return Ok(isValid);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Failed to check if session is valid", exception = ex.Message });
        }
    }

    /// <summary>
    /// Sets HTTP-only cookies for access token and refresh token
    /// </summary>
    private void SetTokenCookies(string accessToken, string refreshToken, DateTimeOffset accessTokenExpires, DateTimeOffset refreshTokenExpires)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Set to true in production with HTTPS
            SameSite = SameSiteMode.Lax,
            Path = "/"
        };

        // Set access token cookie
        cookieOptions.Expires = accessTokenExpires;
        Response.Cookies.Append("accessToken", accessToken, cookieOptions);

        // Set refresh token cookie
        cookieOptions.Expires = refreshTokenExpires;
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    /// <summary>
    /// Clears the token cookies
    /// </summary>
    private void ClearTokenCookies()
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Set to true in production with HTTPS
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(-1) // Expire immediately
        };

        Response.Cookies.Append("accessToken", string.Empty, cookieOptions);
        Response.Cookies.Append("refreshToken", string.Empty, cookieOptions);
    }
}