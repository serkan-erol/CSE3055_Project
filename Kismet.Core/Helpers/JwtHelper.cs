using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Kismet.Core.Helpers
{
    public class JwtHelper
    {
        private readonly IConfiguration _config;

        public JwtHelper(IConfiguration config)
        {
            _config = config;
        }

        // Generate a new access token with JWT for Customer
        public string GenerateCustomerAccessToken(string email, int customerId)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim("CustomerID", customerId.ToString()),
                new Claim("UserType", "Customer"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: GetAccessTokenExpiration().UtcDateTime,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Generate a new access token with JWT for Employee
        public string GenerateEmployeeAccessToken(string email, int employeeId, string role, int accessLevel)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim("EmployeeID", employeeId.ToString()),
                new Claim("UserType", "Employee"),
                new Claim("Role", role),
                new Claim("AccessLevel", accessLevel.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: GetAccessTokenExpiration().UtcDateTime,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Legacy method for backward compatibility (deprecated - use specific methods instead)
        [Obsolete("Use GenerateCustomerAccessToken or GenerateEmployeeAccessToken instead")]
        public string GenerateAccessToken(string email, int userId, string userType)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim("UserId", userId.ToString()),
                new Claim("UserType", userType),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: GetAccessTokenExpiration().UtcDateTime,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Generate a random refresh token encoded in base64
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        // Get the principal from an expired token (used to refresh the access token)
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var jwtSection = _config.GetSection("Jwt");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!)),
                ValidateLifetime = false, // We don't care about the token's expiration date in jwt system
                ValidIssuer = jwtSection["Issuer"],
                ValidAudience = jwtSection["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }

        // Add 15 minutes to the current time to get the access token expiration
        public DateTimeOffset GetAccessTokenExpiration()
        {
            var jwtSection = _config.GetSection("Jwt");
            return DateTimeOffset.Now.AddMinutes(double.Parse(jwtSection["AccessTokenExpireMinutes"] ?? "15"));
        }

        // Add 7 days to the current time to get the refresh token expiration
        public DateTimeOffset GetRefreshTokenExpiration()
        {
            var jwtSection = _config.GetSection("Jwt");
            return DateTimeOffset.Now.AddDays(double.Parse(jwtSection["RefreshTokenExpireDays"] ?? "7"));
        }

        // Get the principal from a token with lifetime validation
        public ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var jwtSection = _config.GetSection("Jwt");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!)),
                ValidateLifetime = true, // Validate token expiration
                ValidIssuer = jwtSection["Issuer"],
                ValidAudience = jwtSection["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }
}