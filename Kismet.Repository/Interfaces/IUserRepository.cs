using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kismet.Entities.DTOs;
using Kismet.Entities.Models;

namespace Kismet.Repository.Interfaces;

public interface IUserRepository
{
    // Entity methods. These are not exposed in the controller!!!
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);
    
    // DTO methods, using DTOs with raw SQL queries
    // Not exposed in the controller
    Task<IReadOnlyList<UserResponseDto>> GetAllDtoAsync(CancellationToken cancellationToken = default);
    Task<UserResponseDto?> GetByIdDtoAsync(int userId, CancellationToken cancellationToken = default);
    // Exposed as end-points in the controller
    Task<UserResponseDto?> UpdateUserNameAsync(UpdateUserNameDto dto, CancellationToken cancellationToken = default);
    Task<UserResponseDto?> UpdateUserEmailAsync(UpdateUserEmailDto dto, CancellationToken cancellationToken = default);
    Task<UserResponseDto?> UpdateUserPhoneAsync(UpdateUserPhoneDto dto, CancellationToken cancellationToken = default);
    Task<UserResponseDto?> UpdateUserPasswordAsync(UpdateUserPasswordDto dto, CancellationToken cancellationToken = default);
    
    // Delete 
    Task<bool> DeleteAsync(int userId, CancellationToken cancellationToken = default);
}

