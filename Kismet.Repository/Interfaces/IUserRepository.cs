using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kismet.Entities.DTOs;
using Kismet.Entities.Models;

namespace Kismet.Repository.Interfaces;

public interface IUserRepository
{
    // DTO methods, using DTOs with raw SQL queries
    // Not exposed in the controller
    Task<IReadOnlyList<UserResponseDto>> GetAllDtoAsync(CancellationToken cancellationToken = default);
    Task<UserResponseDto?> GetByIdDtoAsync(int userId, CancellationToken cancellationToken = default);
    // Exposed as end-points in the controller
    Task<GetUserByEmailResponseDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UserResponseDto?> UpdateUserNameAsync(UpdateUserNameDto dto, CancellationToken cancellationToken = default);
    Task<UserResponseDto?> UpdateUserEmailAsync(UpdateUserEmailDto dto, CancellationToken cancellationToken = default);
    Task<UserResponseDto?> UpdateUserPhoneAsync(UpdateUserPhoneDto dto, CancellationToken cancellationToken = default);
    Task<UserResponseDto?> UpdateUserPasswordAsync(UpdateUserPasswordDto dto, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserPasswordToHashAsync(int userId, string passwordHash, CancellationToken cancellationToken = default);
    // Delete 
    Task<bool> DeleteAsync(int userId, CancellationToken cancellationToken = default);
}