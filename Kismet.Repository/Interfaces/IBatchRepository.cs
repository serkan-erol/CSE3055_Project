using Kismet.Entities.DTOs;

namespace Kismet.Repository.Interfaces;


public interface IBatchRepository
{
    Task<IReadOnlyList<BatchResponseDto>> GetBatchesByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchResponseDto>> GetBatchesByShipmentIdAsync(int shipmentId, CancellationToken cancellationToken = default);
    Task<BatchResponseDto> GetBatchByIdAsync(int batchId, CancellationToken cancellationToken = default);
    Task<CreateBatchesResponseDto> CreateBatchesForMultipleFabricsAsync(int orderId, List<int> fabricIDs, List<int> quantities, List<string?>? qualityGrades, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchResponseDto>> GetUnshippedBatchesByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task<decimal> CalculateOrderTotalAmountAsync(int orderId, CancellationToken cancellationToken = default);
}