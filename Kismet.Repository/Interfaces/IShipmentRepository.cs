using Kismet.Entities.DTOs;
using Kismet.Entities.Enums;
namespace Kismet.Repository.Interfaces;

public interface IShipmentRepository
{
    Task<IReadOnlyList<ShipmentResponseToCustomerDto>> GetShipmentsByOrderIdForCustomerAsync(int orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ShipmentResponseToEmployeeDto>> GetShipmentsByOrderIdForEmployeeAsync(int orderId, CancellationToken cancellationToken = default);
    Task<ShipmentResponseToCustomerDto> GetShipmentByIdForCustomerAsync(int shipmentId, CancellationToken cancellationToken = default);
    Task<ShipmentResponseToEmployeeDto> GetShipmentByIdForEmployeeAsync(int shipmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ShipOrderResponseDto>> ShipOrderAsync(ShipOrderDto dto, CancellationToken cancellationToken = default);
    Task<ShipmentResponseToEmployeeDto> UpdateExpectedDeliveryDateAsync(UpdateExpectedDeliveryDateDto dto, CancellationToken cancellationToken = default);
    Task<ShipmentResponseToEmployeeDto> UpdateShipmentStatusAsync(UpdateShipmentStatusDto dto, CancellationToken cancellationToken = default);
    Task<ShipmentResponseToEmployeeDto> SetActualDeliveryDateAsync(SetActualDeliveryDateDto dto, CancellationToken cancellationToken = default);
    Task<ShipmentResponseToEmployeeDto> UpdateCustomsDocRefAsync(UpdateCustomsDocRefDto dto, CancellationToken cancellationToken = default);
    Task<ShipmentResponseToEmployeeDto> LockShipmentAsync(LockShipmentDto dto, CancellationToken cancellationToken = default);
    Task<bool> CheckIfShipmentIsLockedAsync(int shipmentId, CancellationToken cancellationToken = default);
    Task<ShipmentStatus> GetShipmentStatusByIdAsync(int shipmentId, CancellationToken cancellationToken = default);
    Task<string> GetShipmentStatusDisplayNameByIdAsync(int shipmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetBatchIdsByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);
}