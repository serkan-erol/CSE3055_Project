using Dapper;
using BCrypt.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Kismet.Core.Data;
using Kismet.Core.Helpers;
using Kismet.Entities.DTOs;
using Kismet.Entities.Models;
using Kismet.Repository.Helpers;
using Kismet.Repository.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Kismet.Repository.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IUserRepository _userRepository;
    private readonly JwtHelper _jwtHelper;

    public SessionRepository(IDbConnectionFactory connectionFactory, 
                             IUserRepository userRepository, JwtHelper jwtHelper)
    {
        _connectionFactory = connectionFactory;
        _userRepository = userRepository;
        _jwtHelper = jwtHelper;
    }
    
    /// <summary>
    /// Get session info by ID
    /// </summary>
    public async Task<SessionResponseDto?> GetByIdDtoAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        // Build the query dynamically based on the parameters
        var query = SqlQueries.Session.GetSessionInfoBase + " WHERE s.SessionID = @SessionID";

        // Execute the query and return the session
        return await connection.QueryFirstOrDefaultAsync<SessionResponseDto>(
            new CommandDefinition(query, new { SessionID = sessionId }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Get session info for a User
    /// </summary>
    public async Task<SessionResponseDto?> GetSessionInfoByUserIdDtoAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        // Build the query dynamically based on the parameters
        var query = SqlQueries.Session.GetSessionInfoBase + " WHERE u.UserID = @UserID";

        // Execute the query and return the session
        return await connection.QueryFirstOrDefaultAsync<SessionResponseDto>(
            new CommandDefinition(query, new { UserID = userId }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Get tokens for a session
    /// </summary>
    public async Task<TokenResponseDto?> GetTokenResponseBySessionIdDtoAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        // Build the query dynamically based on the parameters
        var query = SqlQueries.Session.GetTokenResponseBaseDto + " WHERE s.SessionID = @SessionID";

        // Execute the query and return the tokens
        return await connection.QueryFirstOrDefaultAsync<TokenResponseDto>(
            new CommandDefinition(query, new { SessionID = sessionId }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Get tokens for a User
    /// </summary>
    public async Task<TokenResponseDto?> GetTokenResponseByUserIdDtoAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        // Build the query dynamically based on the parameters
        var query = SqlQueries.Session.GetTokenResponseBaseDto + " WHERE u.UserID = @UserID";

        // Execute the query and return the tokens
        return await connection.QueryFirstOrDefaultAsync<TokenResponseDto>(
            new CommandDefinition(query, new { UserID = userId }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Create a new session
    /// </summary>
    public async Task<SessionResponseDto> CreateAsync(CreateSessionDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            // Get the user by ID
            var user = await _userRepository.GetByIdDtoAsync(dto.UserID, cancellationToken);
            if (user is null)
            {
                throw new InvalidOperationException("User not found");
            }

            // Get the user type and generate the AccessToken
            var userType = user.UserType;

            // Generate the AccessToken and RefreshToken
            var accessToken = _jwtHelper.GenerateAccessToken(dto.Email, dto.UserID, userType);
            var refreshToken = _jwtHelper.GenerateRefreshToken();

            // Insert the session and return the session ID
            var sessionId = await connection.QuerySingleAsync<int>(
                new CommandDefinition(SqlQueries.Session.InsertSession, 
                    new 
                    { 
                        dto.UserID, 
                        AccessToken = accessToken, 
                        RefreshToken = refreshToken, 
                        ATExpiresAt = _jwtHelper.GetAccessTokenExpiration(), 
                        RTExpiresAt = _jwtHelper.GetRefreshTokenExpiration() 
                    }, transaction, cancellationToken: cancellationToken));
            await transaction.CommitAsync(cancellationToken);

            // Retrieve the created session using a new connection after transaction is committed and disposed
            return await GetByIdDtoAsync(sessionId, cancellationToken)
                ?? throw new InvalidOperationException("Failed to retrieve created session");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Update the AccessToken for a session
    /// </summary>
    public async Task<TokenResponseDto?> UpdateAccessTokenAsync(UpdateTokensDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // Update the AccessToken and return the session
            await connection.ExecuteAsync(
                new CommandDefinition(SqlQueries.Session.UpdateAccessToken, new { dto.AccessToken, dto.ATExpiresAt, SessionID = dto.SessionID }, transaction, cancellationToken: cancellationToken));
            await transaction.CommitAsync(cancellationToken);
            return await GetTokenResponseBySessionIdDtoAsync(dto.SessionID, cancellationToken)
                ?? throw new InvalidOperationException("Failed to retrieve updated access token");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Update the RefreshToken for a session
    /// </summary>
    public async Task<TokenResponseDto?> UpdateRefreshTokenAsync(UpdateTokensDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // Update the RefreshToken and return the session
            await connection.ExecuteAsync(
                new CommandDefinition(SqlQueries.Session.UpdateRefreshToken, new { dto.RefreshToken, dto.RTExpiresAt, SessionID = dto.SessionID }, transaction, cancellationToken: cancellationToken));
            await transaction.CommitAsync(cancellationToken);
            return await GetTokenResponseBySessionIdDtoAsync(dto.SessionID, cancellationToken)
                ?? throw new InvalidOperationException("Failed to retrieve updated refresh token");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Expire the AccessToken for a session
    /// </summary>
    public async Task<TokenResponseDto?> ExpireAccessTokenAsync(UpdateTokensDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // Expire the AccessToken by setting the ATExpiresAt to the current time
            await connection.ExecuteAsync(
                new CommandDefinition(SqlQueries.Session.ExpireAccessToken, new { dto.AccessToken, dto.ATExpiresAt, SessionID = dto.SessionID }, transaction, cancellationToken: cancellationToken));
            await transaction.CommitAsync(cancellationToken);
            return await GetTokenResponseBySessionIdDtoAsync(dto.SessionID, cancellationToken)
                ?? throw new InvalidOperationException("Failed to retrieve expired access token");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Check if a session is valid
    /// </summary>
    public async Task<bool> IsSessionValidAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        try
        {
            // Check if the session for the user is valid
            var query = SqlQueries.Session.IsSessionValidBase + " WHERE u.UserID = @UserID";
            var isValid = await connection.QueryFirstOrDefaultAsync<bool>(
                new CommandDefinition(query, new { UserID = userId }, cancellationToken: cancellationToken));
            
            return isValid;
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Verify Password
    /// </summary>
    public async Task<bool> VerifyPasswordAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        try
        {
            // Get the user by email
            var user = await _userRepository.GetByEmailAsync(dto.ContactEmail, cancellationToken);
            if (user is null)
            {
                throw new Exception("User not found");
            }

            // Verify the password
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

            // Return the result
            return isPasswordValid;
        }
        catch
        {
            throw;
        }
    }
}