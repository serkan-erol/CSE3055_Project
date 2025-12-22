using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kismet.Entities.DTOs;
using Kismet.Entities.Enums;

namespace Kismet.Repository.Interfaces;

public interface IFinancialTransactionRepository
{
    Task<IReadOnlyList<FTResponseToCustomerDto>> GetFTForCustomerAsync(int customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FTResponseToEmployeeDto>> GetFTForEmployeeAsync(CancellationToken cancellationToken = default);
    Task<FTResponseToCustomerDto> GetFTByIdForCustomerAsync(int customerId, int fTransactionId, CancellationToken cancellationToken = default);
    Task<FTResponseToEmployeeDto> GetFTByIdForEmployeeAsync(int fTransactionId, CancellationToken cancellationToken = default);
    Task<PaymentStatus> GetFTPaymentStatusAsync(int fTransactionId, CancellationToken cancellationToken = default);
    Task<FTResponseToCustomerDto> CreateFTAsync(CreateFTDto dto, CancellationToken cancellationToken = default);
    Task<FTResponseToCustomerDto> UpdateFTTotalPaidAsync(int customerId, int fTransactionId, UpdateFTTotalPaidDto dto, CancellationToken cancellationToken = default);
    Task<FTResponseToCustomerDto> UpdateFTDescriptionAsync(int customerId, UpdateFTDescriptionDto dto, CancellationToken cancellationToken = default);
    Task<BillingResponseToEmployeeDto?> FindSuitableBillingEntryForFTAsync(CreateFTDto dto, CancellationToken cancellationToken = default);
    Task<bool> IsBillingEntrySuitableForFTAsync(CreateFTDto dto, CancellationToken cancellationToken = default);
}