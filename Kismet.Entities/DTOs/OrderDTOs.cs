using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Kismet.Entities.Enums;

namespace Kismet.Entities.DTOs;

/// <summary>
/// DTO for returning Order data to API clients that are customers
/// </summary>
public class OrderResponseToCustomerDto
{
    public int OrderID { get; set; }
    public int CustomerID { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public DateTimeOffset OrderDate { get; set; }
    public DateTimeOffset? LastUpdatedAt { get; set; }
}

/// <summary>
/// DTO for returning Order data to API clients that are employees
/// </summary>
public class OrderResponseToEmployeeDto
{
    public int OrderID { get; set; }
    public int CustomerID { get; set; }
    public bool ReliabilityStatus { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public bool IsApproved { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovalDate { get; set; }
    public bool IsLocked { get; set; }
    public DateTimeOffset? LockedAt { get; set; }
    public DateTimeOffset OrderDate { get; set; }
    public DateTimeOffset? LastUpdatedAt { get; set; }
}

/// <summary>
/// DTO for returning OrderStatus
/// </summary>
public class OrderStatusResponseDto
{
    public OrderStatus OrderStatus { get; set; }
}

/// <summary>
/// DTO for creating a new Order
/// </summary>
public class CreateOrderDto
{
    [Required]
    [JsonIgnore]
    public int CustomerID { get; set; }

    [Required]
    public string OrderType { get; set; } = string.Empty; // Purchase or Supply

    [Required]
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// DTO for updating OrderStatus
/// </summary>
public class UpdateOrderStatusDto
{
    [Required]
    [JsonIgnore]
    public int ApprovedBy { get; set; }

    // Set by controller from route; not accepted from request body
    public int OrderID { get; set; }

    [EnumDataType(typeof(OrderStatus))]
    public OrderStatus OrderStatus { get; set; }
}

/// <summary>
/// DTO for approving an order
/// </summary>
public class ApproveOrderDto
{
    [Required]
    [JsonIgnore]
    public int ApprovedBy { get; set; }

    // Set by controller from route; not accepted from request body
    public int OrderID { get; set; }
}