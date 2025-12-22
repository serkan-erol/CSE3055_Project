using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kismet.Entities.DTOs;
using Kismet.Entities.Enums;

namespace Kismet.Repository.Interfaces;

public interface IBillingRepository
{
    Task<IReadOnlyList<BillingResponseToCustomerDto>> GetBillingForCustomerAsync(int customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BillingResponseToEmployeeDto>> GetBillingForEmployeeAsync(CancellationToken cancellationToken = default);
    Task<BillingResponseToCustomerDto> GetBillingByIdForCustomerAsync(int customerId, int billingId, CancellationToken cancellationToken = default);
    Task<BillingResponseToEmployeeDto> GetBillingByIdForEmployeeAsync(int billingId, CancellationToken cancellationToken = default);
    Task<BillingResponseToCustomerDto> CreateBillingAsync(CreateBillingDto dto, CancellationToken cancellationToken = default);
    Task<BillingResponseToEmployeeDto> UpdateBillingTypeAsync(UpdateBillingTypeDto dto, CancellationToken cancellationToken = default);
    Task<BillingResponseToCustomerDto> UpdateBillingAsync(int customerId, int billingId, /*int paymentId,*/ UpdateBillingDto dto, CancellationToken cancellationToken = default);
    Task<BillingResponseToCustomerDto> UpdateBillingPaymentTermsAsync(UpdateBillingPaymentTermsDto dto, CancellationToken cancellationToken = default);
    Task<PaymentStatus> GetBillingStatusByIdAsync(int billingId, CancellationToken cancellationToken = default);
    Task<string> GetBillingStatusDisplayNameByIdAsync(int billingId, CancellationToken cancellationToken = default);
}