using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kismet.Entities.DTOs;
using Kismet.Entities.Enums;

namespace Kismet.Repository.Interfaces;

public interface IOrderRepository
{
    Task<IReadOnlyList<OrderResponseToCustomerDto>> GetOrderForCustomerAsync(int customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderResponseToEmployeeDto>> GetOrderForEmployeeAsync(CancellationToken cancellationToken = default);
    Task<OrderResponseToCustomerDto> GetOrderByOrderIdForCustomerAsync(int orderId, int customerId, CancellationToken cancellationToken = default);
    Task<OrderResponseToEmployeeDto> GetOrderByOrderIdForEmployeeAsync(int orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderResponseToEmployeeDto>> GetOrderByCustomerIdForEmployeeAsync(int customerId, CancellationToken cancellationToken = default);
    Task<OrderResponseToCustomerDto> CreateOrderAsync(CreateOrderDto dto, CancellationToken cancellationToken = default);
    Task<OrderResponseToEmployeeDto> UpdateOrderStatusAsync(UpdateOrderStatusDto dto, CancellationToken cancellationToken = default);
    Task<OrderResponseToEmployeeDto> ApproveOrderAsync(ApproveOrderDto dto, CancellationToken cancellationToken = default);
    Task<OrderStatus> GetOrderStatusByIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task<string> GetOrderStatusDisplayNameByIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task<bool> CheckIfOrderIsLockedAsync(int orderId, CancellationToken cancellationToken = default);
}