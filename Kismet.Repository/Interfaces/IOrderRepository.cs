using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kismet.Entities.DTOs;
using Kismet.Entities.Enums;

namespace Kismet.Repository.Interfaces;

public interface IOrderRepository
{
    Task<IReadOnlyList<OrderResponseToCustomerDto>> GetOrderForCustomerAsync(int customerId, CancellationToken cancellationToken = default);
    Task<OrderResponseToCustomerDto> GetOrderByIdForCustomerAsync(int customerId, int orderId, CancellationToken cancellationToken = default);
    Task<OrderResponseToCustomerDto?> GetOrderByOrderNumberForCustomerAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<OrderResponseToEmployeeDto> GetOrderByIdForEmployeeAsync(int orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderResponseToEmployeeDto>> GetOrderByCustomerIdForEmployeeAsync(int customerId, CancellationToken cancellationToken = default);
    Task<OrderResponseToEmployeeDto?> GetOrderByOrderNumberForEmployeeAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderResponseToEmployeeDto>> GetOrderByCustomerNumberForEmployeeAsync(string customerNumber, CancellationToken cancellationToken = default);
    Task<OrderResponseToCustomerDto> CreateOrderAsync(CreateOrderDto dto, CancellationToken cancellationToken = default);
    Task<OrderResponseToEmployeeDto> UpdateOrderStatusAsync(UpdateOrderStatusDto dto, CancellationToken cancellationToken = default);
    Task<OrderResponseToEmployeeDto> CancelOrderAsync(int orderId, CancellationToken cancellationToken = default);
    Task<OrderResponseToEmployeeDto> ApproveOrderAsync(ApproveOrderDto dto, CancellationToken cancellationToken = default);
    Task<OrderStatus> GetOrderStatusByIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task<string> GetOrderStatusDisplayNameByIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task<bool> CheckIfOrderIsLockedAsync(int orderId, CancellationToken cancellationToken = default);
}