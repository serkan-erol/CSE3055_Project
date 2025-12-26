namespace Kismet.Entities.DTOs;

/// <summary>
/// DTO for vw_OrderDetails - filter by CustomerID
/// </summary>
public class OrderDetailsViewDto
{
    public int OrderID { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public int OrderStatus { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTimeOffset OrderDate { get; set; }
    
    public int CustomerID { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Country { get; set; }
    
    public string? ApprovedByEmployeeName { get; set; }
    public DateTimeOffset? ApprovalDate { get; set; }
    
    public int TotalBatches { get; set; }
    public int TotalQuantity { get; set; }
    public string ShipmentStatusSummary { get; set; } = string.Empty;
    public int TotalShipments { get; set; }
}

/// <summary>
/// DTO for vw_ShipmentTracking - filter by CustomerID
/// </summary>
public class ShipmentTrackingViewDto
{
    public int ShipmentID { get; set; }
    public int OrderID { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string ShipmentStatusText { get; set; } = string.Empty;
    public DateTimeOffset? ShipmentDate { get; set; }
    public DateTimeOffset? ExpectedDeliveryDate { get; set; }
    public DateTimeOffset? ActualDeliveryDate { get; set; }
    
    public int CustomerID { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerCity { get; set; }
    
    public int TotalBatches { get; set; }
    public int TotalUnits { get; set; }
    public decimal TotalValue { get; set; }
    
    public int? DeliveryDelayDays { get; set; }
    public int? DaysInTransit { get; set; }
}

/// <summary>
/// DTO for vw_FinancialOverview - filter by CustomerID
/// </summary>
public class FinancialOverviewViewDto
{
    public int FTransactionID { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal RemainingBalance { get; set; }
    public string PaymentStatusText { get; set; } = string.Empty;
    public DateTimeOffset TransactionDate { get; set; }
    
    public int CustomerID { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    
    public int OrderID { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    
    public string InvoiceNumber { get; set; } = string.Empty;
    
    public decimal? TreasuryAmount { get; set; }
    public decimal? TreasuryBalanceAfter { get; set; }
    
    public int DaysUntilDue { get; set; }
    public string TransactionStatus { get; set; } = string.Empty;
    public decimal PaymentPercentage { get; set; }
}

/// <summary>
/// DTO for vw_InventoryProductionStatus - filter by FabricID
/// </summary>
public class InventoryProductionStatusViewDto
{
    public int FabricID { get; set; }
    public string FabricType { get; set; } = string.Empty;
    public string? Color { get; set; }
    public int CurrentStock { get; set; }
    
    public int TotalBatchesProduced { get; set; }
    public int UnshippedBatches { get; set; }
    public int ShippedBatches { get; set; }
    
    public int TotalQuantityUsed { get; set; }
    public int UnshippedQuantity { get; set; }
    public int ShippedQuantity { get; set; }
    
    public decimal TotalRevenue { get; set; }
    public decimal AverageBatchPrice { get; set; }
    
    public int TotalOrders { get; set; }
    public DateTimeOffset? LatestProductionDate { get; set; }
    
    public string StockStatus { get; set; } = string.Empty;
    public decimal AvgQuantityPerBatch { get; set; }
}