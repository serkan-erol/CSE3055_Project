using Dapper;
using Kismet.Core.Data;
using Kismet.Entities.DTOs;
using Kismet.Entities.Models;
using Kismet.Repository.Helpers;
using Kismet.Repository.Interfaces;

namespace Kismet.Repository.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ICustomerRepository _customerRepository;

    public OrderRepository(IDbConnectionFactory connectionFactory, ICustomerRepository customerRepository)
    {
        _connectionFactory = connectionFactory;
        _customerRepository = customerRepository;
    }

    public async Task<IReadOnlyList<OrderResponseToCustomerDto>> GetOrderForCustomerAsync(int customerId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var orders = await connection.QueryAsync<OrderResponseToCustomerDto>(
            new CommandDefinition(SqlQueries.Order.GetOrderForCustomer, 
                new { CustomerID = customerId }, 
                cancellationToken: cancellationToken));
        return orders.ToList().AsReadOnly();
    }

    // DTO methods, using DTOs with raw SQL queries
    public async Task<IReadOnlyList<OrderResponseToEmployeeDto>> GetOrderForEmployeeAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var orders = await connection.QueryAsync<OrderResponseToEmployeeDto>(
            new CommandDefinition(SqlQueries.Order.GetOrderForEmployee, cancellationToken: cancellationToken));
        return orders.ToList().AsReadOnly();
    }

    /// <summary>
    /// Get an order by ID for a customer
    /// </summary>
    public async Task<OrderResponseToCustomerDto> GetOrderByOrderIdForCustomerAsync(int orderId, int customerId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var order = await connection.QuerySingleOrDefaultAsync<OrderResponseToCustomerDto>(
            new CommandDefinition(SqlQueries.Order.GetOrderByOrderIdForCustomer, new { OrderID = orderId, CustomerID = customerId }, cancellationToken: cancellationToken));
        return order ?? throw new InvalidOperationException("Order not found");
    }

    /// <summary>
    /// Get an order by ID for an employee
    /// </summary>
    public async Task<OrderResponseToEmployeeDto> GetOrderByOrderIdForEmployeeAsync(int orderId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var order = await connection.QuerySingleOrDefaultAsync<OrderResponseToEmployeeDto>(
            new CommandDefinition(SqlQueries.Order.GetOrderByOrderIdForEmployee, new { OrderID = orderId }, cancellationToken: cancellationToken));
        return order ?? throw new InvalidOperationException("Order not found");
    }

    /// <summary>
    /// Get an order by Customer ID for an employee
    /// </summary>
    public async Task<IReadOnlyList<OrderResponseToEmployeeDto>> GetOrderByCustomerIdForEmployeeAsync(int customerId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var orders = await connection.QueryAsync<OrderResponseToEmployeeDto>(
            new CommandDefinition(SqlQueries.Order.GetOrderByCustomerIdForEmployee, new { CustomerID = customerId }, cancellationToken: cancellationToken));
        return orders.ToList().AsReadOnly();
    }
    
    /// <summary>
    /// Create a new order
    /// </summary>
    public async Task<OrderResponseToCustomerDto> CreateOrderAsync(CreateOrderDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var orderId = await connection.QuerySingleAsync<int>(
            new CommandDefinition(SqlQueries.Order.InsertOrder, dto, cancellationToken: cancellationToken));
        
        // Use the existing method to retrieve the order, just like Customer/Employee do
        return await GetOrderByOrderIdForCustomerAsync(orderId, dto.CustomerID, cancellationToken);
    }

    /// <summary>
    /// Update the status of an order (only employees can update order status)
    /// </summary>
    public async Task<OrderResponseToEmployeeDto> UpdateOrderStatusAsync(UpdateOrderStatusDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        // Update the order status
        await connection.ExecuteAsync(
            new CommandDefinition(SqlQueries.Order.UpdateOrderStatus, dto, cancellationToken: cancellationToken));
        
        // Return the updated order using the employee DTO
        return await GetOrderByOrderIdForEmployeeAsync(dto.OrderID, cancellationToken);
    }

    /// <summary>
    /// Approve an order
    /// </summary>
    public async Task<OrderResponseToEmployeeDto> ApproveOrderAsync(ApproveOrderDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        // Approve the order
        await connection.ExecuteAsync(
            new CommandDefinition(SqlQueries.Order.ApproveOrder, dto, cancellationToken: cancellationToken));
        
        // Return the approved order
        return await GetOrderByOrderIdForEmployeeAsync(dto.OrderID, cancellationToken);
    }

    /// <summary>
    /// Get the status of an order
    /// </summary>
    public async Task<OrderStatus> GetOrderStatusByIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var orderStatus = await connection.QuerySingleOrDefaultAsync<OrderStatus>(
            new CommandDefinition(SqlQueries.Order.GetOrderStatusById, new { OrderID = orderId }, cancellationToken: cancellationToken));

        return orderStatus;
    }

    /// <summary>
    /// Check if an order is locked for updates
    /// </summary>
    public async Task<bool> CheckIfOrderIsLockedAsync(int orderId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var isLocked = await connection.QuerySingleOrDefaultAsync<bool>(
            new CommandDefinition(SqlQueries.Order.CheckIfOrderIsLocked, new { OrderID = orderId }, cancellationToken: cancellationToken));
        return isLocked;
    }
}