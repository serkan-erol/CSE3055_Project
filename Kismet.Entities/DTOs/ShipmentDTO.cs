using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Kismet.Entities.Enums;

namespace Kismet.Entities.DTOs;

/// <summary>
/// DTO for returning Shipment data to API clients that are customers
/// </summary>
public class ShipmentResponseToCustomerDto
{
    public int ShipmentID { get; set; }
    public ShipmentStatus ShipmentStatus { get; set; }
    public DateTimeOffset? ShipmentDate { get; set; }
    public string? OriginCountry { get; set; }
    public string? DestinationCountry { get; set; }
    public DateTimeOffset? ExpectedDeliveryDate { get; set; }
    public DateTimeOffset? ActualDeliveryDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>
/// DTO for returning Shipment data to API clients that are employees
/// </summary>
public class ShipmentResponseToEmployeeDto
{
    public int ShipmentID { get; set; }
    public int OrderID { get; set; }
    public string? CustomsDocRef { get; set; }
    public ShipmentStatus ShipmentStatus { get; set; }
    public bool IsLocked { get; set; }
    public DateTimeOffset? LockedAt { get; set; }
    public DateTimeOffset? ShipmentDate { get; set; }
    public string? OriginCountry { get; set; }
    public string? DestinationCountry { get; set; }
    public DateTimeOffset? ExpectedDeliveryDate { get; set; }
    public DateTimeOffset? ActualDeliveryDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastUpdatedAt { get; set; }
}

/// <summary>
/// DTO for shipOrder method - internal use only
/// </summary>
public class ShipOrderDto
{
    [Required]
    public int OrderID { get; set; }

    [Required]
    [JsonIgnore]
    public int EmployeeID { get; set; }
}

/// <summary>
/// DTO for shipOrder response with delivery info
/// </summary>
public class ShipOrderResponseDto
{
    public int ShipmentID { get; set; }
    public DateTimeOffset? ExpectedDeliveryDate { get; set; }
    public int BatchCount { get; set; }
}

/// <summary>
/// DTO for updating expected delivery date
/// </summary>
public class UpdateExpectedDeliveryDateDto
{
    [Required]
    public int ShipmentID { get; set; }

    [Required]
    public DateTimeOffset ExpectedDeliveryDate { get; set; }

    [Required]
    [JsonIgnore]
    public int EmployeeID { get; set; }
}

/// <summary>
/// DTO for updating shipment status
/// </summary>
public class UpdateShipmentStatusDto
{
    [Required]
    public int ShipmentID { get; set; }

    [Required]
    [EnumDataType(typeof(ShipmentStatus))]
    public ShipmentStatus ShipmentStatus { get; set; }
}

/// <summary>
/// DTO for setting actual delivery date
/// </summary>
public class SetActualDeliveryDateDto
{
    [Required]
    public int ShipmentID { get; set; }

    [Required]
    public DateTimeOffset ActualDeliveryDate { get; set; }
}

/// <summary>
/// DTO for updating customs document reference
/// </summary>
public class UpdateCustomsDocRefDto
{
    [Required]
    public int ShipmentID { get; set; }

    [Required]
    public string CustomsDocRef { get; set; } = string.Empty;
}

/// <summary>
/// DTO for locking/unlocking shipment
/// </summary>
public class LockShipmentDto
{
    [Required]
    public int ShipmentID { get; set; }
}