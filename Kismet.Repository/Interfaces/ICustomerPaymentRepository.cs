using Kismet.Entities.DTOs;

namespace Kismet.Repository.Interfaces;

public interface ICustomerPaymentRepository
{
    // Saved Payment Methods
    Task<IReadOnlyList<SavedPaymentMethodResponseDto>> GetSavedPaymentMethodsByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
    Task<SavedPaymentMethodResponseDto?> GetSavedPaymentMethodByIdAsync(int spmId, CancellationToken cancellationToken = default);
    Task<SavedPaymentMethodResponseDto> CreateSavedPaymentMethodAsync(CreateSavedPaymentMethodDto dto, CancellationToken cancellationToken = default);
    Task<SavedPaymentMethodResponseDto> UpdateSavedPaymentMethodAsync(int spmId, UpdateSavedPaymentMethodDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteSavedPaymentMethodAsync(int spmId, CancellationToken cancellationToken = default);
    
    // Saved Bank Information
    Task<IReadOnlyList<SavedBankInformationResponseDto>> GetSavedBankInformationByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
    Task<SavedBankInformationResponseDto?> GetSavedBankInformationByIdAsync(int sbiId, CancellationToken cancellationToken = default);
    Task<SavedBankInformationResponseDto> CreateSavedBankInformationAsync(CreateSavedBankInformationDto dto, CancellationToken cancellationToken = default);
    Task<SavedBankInformationResponseDto> UpdateSavedBankInformationAsync(int sbiId, UpdateSavedBankInformationDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteSavedBankInformationAsync(int sbiId, CancellationToken cancellationToken = default);
}
