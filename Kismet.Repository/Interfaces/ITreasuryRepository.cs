using Kismet.Entities.DTOs;

namespace Kismet.Repository.Interfaces;

public interface ITreasuryRepository
{
    Task<IReadOnlyList<TreasuryResponseDto>> GetAllTreasuryEntriesAsync(CancellationToken cancellationToken = default);
    Task<TreasuryResponseDto> GetTreasuryEntryByIdAsync(int treasuryId, CancellationToken cancellationToken = default);
    Task<TreasuryBalanceDto> GetCurrentBalanceAsync(CancellationToken cancellationToken = default);
    // Task<TreasuryResponseDto> CreateTreasuryEntryAsync(CreateTreasuryEntryDto dto, CancellationToken cancellationToken = default); // Now handled by trigger
    Task<IReadOnlyList<TreasuryResponseDto>> GetTreasuryEntriesByTransactionIdAsync(int fTransactionId, CancellationToken cancellationToken = default);
}