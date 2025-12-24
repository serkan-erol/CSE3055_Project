using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kismet.Entities.DTOs;
using Kismet.Entities.Models;

namespace Kismet.Repository.Interfaces;

public interface ISessionRepository
{
    Task<SessionResponseDto?> GetByIdDtoAsync(int sessionId, CancellationToken cancellationToken = default);
    Task<SessionResponseDto?> GetSessionInfoByUserIdDtoAsync(int userId, CancellationToken cancellationToken = default);
    Task<TokenResponseDto?> GetTokenResponseBySessionIdDtoAsync(int sessionId, CancellationToken cancellationToken = default);
    Task<TokenResponseDto?> GetTokenResponseByUserIdDtoAsync(int userId, CancellationToken cancellationToken = default);
    Task<SessionResponseDto> CreateAsync(CreateSessionDto dto, CancellationToken cancellationToken = default);
    Task<TokenResponseDto?> UpdateAccessTokenAsync(UpdateTokensDto dto, CancellationToken cancellationToken = default);
    Task<TokenResponseDto?> UpdateRefreshTokenAsync(UpdateTokensDto dto, CancellationToken cancellationToken = default);
    Task<TokenResponseDto?> ExpireAccessTokenAsync(UpdateTokensDto dto, CancellationToken cancellationToken = default);
    Task<bool> IsSessionValidAsync(int sessionId, CancellationToken cancellationToken = default);
    Task<bool> VerifyPasswordAsync(LoginDto dto, CancellationToken cancellationToken = default);
}