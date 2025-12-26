using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kismet.Entities.DTOs;
using Kismet.Entities.Models;

namespace Kismet.Repository.Interfaces;

public interface IEmployeeRepository
{
    // DTO methods, using DTOs with raw SQL queries
    Task<IReadOnlyList<EmployeeResponseDto>> GetAllDtoAsync(CancellationToken cancellationToken = default);
    Task<EmployeeResponseDto?> GetByIdDtoAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<EmployeeResponseDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<EmployeeResponseDto?> GetByEmployeeNumberAsync(string employeeNumber, CancellationToken cancellationToken = default);
    Task<EmployeeResponseDto> CreateAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default);
    Task<EmployeeResponseDto?> UpdateEmployeeRoleAsync(UpdateEmployeeRoleDto dto, CancellationToken cancellationToken = default);
    Task<EmployeeResponseDto?> UpdateEmployeeAccessLevelAsync(UpdateEmployeeAccessLevelDto dto, CancellationToken cancellationToken = default);
    
    // Delete 
    Task<bool> DeleteAsync(int employeeId, CancellationToken cancellationToken = default);
}

