using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kismet.Entities.DTOs;
using Kismet.Entities.Enums;

namespace Kismet.Repository.Interfaces;

public interface IPaymentRepository
{
    Task<IReadOnlyList<PaymentResponseDto>> GetAllPaymentsAsync(CancellationToken cancellationToken = default);
    Task<PaymentResponseDto> GetPaymentByIdAsync(int paymentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentResponseDto>> GetPaymentForCustomerAsync(int customerId, CancellationToken cancellationToken = default);
    Task<PaymentResponseDto> GetPaymentByIdForCustomerAsync(int customerId, int paymentId, CancellationToken cancellationToken = default);
    Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto dto, CancellationToken cancellationToken = default);
}