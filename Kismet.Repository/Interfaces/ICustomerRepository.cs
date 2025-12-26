using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kismet.Entities.DTOs;
using Kismet.Entities.Models;

namespace Kismet.Repository.Interfaces;

public interface ICustomerRepository
{
    // DTO methods, using DTOs with raw SQL queries
    Task<IReadOnlyList<CustomerResponseDto>> GetAllDtoAsync(CancellationToken cancellationToken = default);
    Task<CustomerResponseDto?> GetByIdDtoAsync(int customerId, CancellationToken cancellationToken = default);
    Task<CustomerResponseDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<CustomerResponseDto?> GetByCustomerNumberAsync(string customerNumber, CancellationToken cancellationToken = default);
    Task<CustomerResponseDto> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default);
    Task<CustomerResponseDto?> UpdateCustomerTypeAsync(UpdateCustomerTypeDto dto, CancellationToken cancellationToken = default);
    Task<CustomerResponseDto?> UpdateCustomerReliabilityAsync(UpdateCustomerReliabilityDto dto, CancellationToken cancellationToken = default);
    Task<CustomerResponseDto?> UpdateCustomerCityAndCountryAsync(UpdateCustomerCityAndCountryDto dto, CancellationToken cancellationToken = default);
    
    // Delete 
    Task<bool> DeleteAsync(int customerId, CancellationToken cancellationToken = default);
}

