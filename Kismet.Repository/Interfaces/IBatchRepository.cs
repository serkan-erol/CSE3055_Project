using Kismet.Entities.DTOs;

namespace Kismet.Repository.Interfaces;


public interface IBatchRepository
{
    Task<IReadOnlyList<BatchResponseToCustomerDto>> GetBatchesByOrderIdForCustomerAsync(int orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchResponseToEmployeeDto>> GetBatchesByOrderIdForEmployeeAsync(int orderId, CancellationToken cancellationToken = default);
    Task<BatchResponseToEmployeeDto> GetBatchByIdAsync(int batchId, CancellationToken cancellationToken = default);
    Task<CreateBatchesResponseDto> CreateBatchesAsync(CreateBatchesDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchResponseToEmployeeDto>> GetUnshippedBatchesByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);
}