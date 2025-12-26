---------------------------------------------------------------------------------------------------------------
-----------------------------------------------VIEWS-----------------------------------------------------------
---------------------------------------------------------------------------------------------------------------

CREATE VIEW vw_OrderDetails AS
SELECT 
    o.OrderID,
    o.OrderNumber,
    o.OrderType,
    o.OrderStatus,
    o.TotalAmount,
    o.IsApproved,
    o.OrderDate,
    
    -- Customer information
    c.CustomerID,
    u.UserName as CustomerName,
    u.ContactEmail as CustomerEmail,
    c.CustomerNumber,
    c.City,
    c.Country,
    c.ReliabilityStatus,
    
    -- Approval information
    e.EmployeeID as ApprovedByEmployeeID,
    eu.UserName as ApprovedByEmployeeName,
    o.ApprovalDate,
    
    -- Aggregated batch information
    COUNT(DISTINCT b.BatchID) as TotalBatches,
    SUM(b.Quantity) as TotalQuantity,
    COUNT(DISTINCT b.FabricID) as UniqueFabricCount,
    
    -- Shipment status
    CASE 
        WHEN COUNT(DISTINCT s.ShipmentID) = 0 THEN 'Not Shipped'
        WHEN COUNT(DISTINCT CASE WHEN s.ShipmentStatus = 2 THEN s.ShipmentID END) = COUNT(DISTINCT s.ShipmentID) 
            THEN 'Fully Delivered'
        WHEN COUNT(DISTINCT CASE WHEN s.ShipmentStatus >= 1 THEN s.ShipmentID END) > 0 
            THEN 'Partially Shipped'
        ELSE 'Pending'
    END as ShipmentStatusSummary,
    
    COUNT(DISTINCT s.ShipmentID) as TotalShipments

FROM dbo.[Order] o
INNER JOIN dbo.[Customer] c ON c.CustomerID = o.CustomerID
INNER JOIN dbo.[User] u ON u.UserID = c.CustomerID AND u.UserType = 'Customer'
LEFT JOIN dbo.[Employee] e ON e.EmployeeID = o.ApprovedBy
LEFT JOIN dbo.[User] eu ON eu.UserID = e.EmployeeID AND eu.UserType = 'Employee'
LEFT JOIN dbo.[Batch] b ON b.OrderID = o.OrderID
LEFT JOIN dbo.[Shipment] s ON s.OrderID = o.OrderID

GROUP BY 
    o.OrderID, o.OrderNumber, o.OrderType, o.OrderStatus, o.TotalAmount, 
    o.IsApproved, o.OrderDate, c.CustomerID, u.UserName, u.ContactEmail, 
    c.CustomerNumber, c.City, c.Country, c.ReliabilityStatus,
    e.EmployeeID, eu.UserName, o.ApprovalDate;
GO

-------------------------------------------------------------------------------------------------------------
CREATE VIEW vw_ShipmentTracking AS
SELECT 
    s.ShipmentID,
    s.OrderID,
    o.OrderNumber,
    s.ShipmentStatus,
    CASE s.ShipmentStatus
        WHEN 0 THEN 'Pending'
        WHEN 1 THEN 'In Transit'
        WHEN 2 THEN 'Delivered'
        WHEN 3 THEN 'Failed'
    END as ShipmentStatusText,  s.ShipmentDate, s.ExpectedDeliveryDate, s.ActualDeliveryDate, s.OriginCountry,
     s.DestinationCountry,
    s.CustomsDocRef,
    
    -- Customer information
    c.CustomerID,  u.UserName as CustomerName,
    c.City as CustomerCity,  c.Country as CustomerCountry,
    
    -- Batch aggregation
    COUNT(b.BatchID) as TotalBatches,
    SUM(b.Quantity) as TotalUnits,
    SUM(b.BatchPrice) as TotalValue,
    
    -- Delivery performance
    CASE 
        WHEN s.ActualDeliveryDate IS NOT NULL AND s.ExpectedDeliveryDate IS NOT NULL THEN
            DATEDIFF(day, s.ExpectedDeliveryDate, s.ActualDeliveryDate)
        ELSE NULL
    END as DeliveryDelayDays,
    
    -- Days in transit
    CASE 
        WHEN s.ActualDeliveryDate IS NOT NULL AND s.ShipmentDate IS NOT NULL THEN
            DATEDIFF(day, s.ShipmentDate, s.ActualDeliveryDate)
        WHEN s.ShipmentDate IS NOT NULL THEN
            DATEDIFF(day, s.ShipmentDate, CAST(GETDATE() AS date))
        ELSE NULL
    END as DaysInTransit

FROM dbo.[Shipment] s
INNER JOIN dbo.[Order] o ON o.OrderID = s.OrderID
INNER JOIN dbo.[Customer] c ON c.CustomerID = o.CustomerID
INNER JOIN dbo.[User] u ON u.UserID = c.CustomerID AND u.UserType = 'Customer'
LEFT JOIN dbo.[Batch] b ON b.ShipmentID = s.ShipmentID
LEFT JOIN dbo.[Fabric] f ON f.FabricID = b.FabricID

GROUP BY 
    s.ShipmentID, s.OrderID, o.OrderNumber, s.ShipmentStatus, s.ShipmentDate,
    s.ExpectedDeliveryDate, s.ActualDeliveryDate, s.OriginCountry, 
    s.DestinationCountry, s.CustomsDocRef, c.CustomerID, u.UserName,
    c.City, c.Country;
GO

------------------------------------------------------------------------------------------------------------------
CREATE VIEW vw_FinancialOverview AS
SELECT 
    ft.FTransactionID,
    ft.TransactionType,
    ft.TotalAmount,
    ft.TotalPaid,
    ft.RemainingBalance,
    ft.PaymentStatus,
    CASE ft.PaymentStatus
        WHEN 0 THEN 'Unpaid'
        WHEN 1 THEN 'Partial'
        WHEN 2 THEN 'Paid'
    END as PaymentStatusText,
    ft.TransactionDate,
    
    -- Customer information
    c.CustomerID,
    u.UserName as CustomerName,
    c.CustomerNumber,
    c.ReliabilityStatus,
    c.City,
    c.Country,
    
    -- Order information
    o.OrderID,
    o.OrderNumber,
    o.OrderType,
    o.OrderStatus,
    
    -- Billing information
    b.BillingID,
    b.InvoiceNumber,
    b.BillingType,
    b.PaymentTerms,
    b.BillingDate,
    
    -- Treasury impact
    t.TreasuryID,
    t.Amount as TreasuryAmount,
    t.BalanceAfter as TreasuryBalanceAfter,
    t.EntryDate as TreasuryEntryDate,
    
    -- Calculated fields
    DATEDIFF(day, ft.TransactionDate, GETDATE()) as DaysUntilDue,
    CASE 
        WHEN ft.TransactionDate < CAST(GETDATE() AS date) AND ft.RemainingBalance > 0 
            THEN 'Overdue'
        WHEN ft.RemainingBalance = 0 THEN 'Completed'
        ELSE 'Active'
    END as TransactionStatus,
    
    -- Payment percentage
    CASE 
        WHEN ft.TotalAmount > 0 THEN (ft.TotalPaid * 100.0 / ft.TotalAmount)
        ELSE 0
    END as PaymentPercentage

FROM dbo.[FinancialTransaction] ft
INNER JOIN dbo.[Customer] c ON c.CustomerID = ft.CustomerID
INNER JOIN dbo.[User] u ON u.UserID = c.CustomerID AND u.UserType = 'Customer'
INNER JOIN dbo.[Order] o ON o.OrderID = ft.OrderID
INNER JOIN dbo.[Billing] b ON b.BillingID = ft.BillingID
LEFT JOIN dbo.[Treasury] t ON t.FTransactionID = ft.FTransactionID;
GO
-------------------------------------------------------------------------------------------------------------


CREATE VIEW vw_InventoryProductionStatus AS
SELECT 
    f.FabricID,
    f.FabricType,
    f.Composition,
    f.Color,
    f.WeightPerUnit,
    f.StockQuantity as CurrentStock,
    f.Description,
    
    -- Batch statistics
    COUNT(DISTINCT b.BatchID) as TotalBatchesProduced,
    SUM(CASE WHEN b.ShipmentID IS NULL THEN 1 ELSE 0 END) as UnshippedBatches,
    SUM(CASE WHEN b.ShipmentID IS NOT NULL THEN 1 ELSE 0 END) as ShippedBatches,
    
    -- Quantity statistics
    ISNULL(SUM(b.Quantity), 0) as TotalQuantityUsed,
    ISNULL(SUM(CASE WHEN b.ShipmentID IS NULL THEN b.Quantity ELSE 0 END), 0) as UnshippedQuantity,
    ISNULL(SUM(CASE WHEN b.ShipmentID IS NOT NULL THEN b.Quantity ELSE 0 END), 0) as ShippedQuantity,
    
    -- Revenue statistics
    ISNULL(SUM(b.BatchPrice), 0) as TotalRevenue,
    ISNULL(AVG(b.BatchPrice), 0) as AverageBatchPrice,
    
    -- Order statistics
    COUNT(DISTINCT b.OrderID) as TotalOrders,
    COUNT(DISTINCT CASE WHEN b.ShipmentID IS NULL THEN b.OrderID END) as PendingOrders,

    -- Latest production date
    MAX(b.ProductionDate) as LatestProductionDate,
    
    -- Stock status
    CASE 
        WHEN f.StockQuantity = 0 THEN 'Out of Stock'
        WHEN f.StockQuantity < 100 THEN 'Low Stock'
        WHEN f.StockQuantity < 500 THEN 'Medium Stock'
        ELSE 'Well Stocked'
    END as StockStatus,
    
    -- Average utilization per batch
    CASE 
        WHEN COUNT(b.BatchID) > 0 THEN 
            CAST(SUM(b.Quantity) AS decimal(18,2)) / COUNT(b.BatchID)
        ELSE 0
    END as AvgQuantityPerBatch

FROM dbo.[Fabric] f
LEFT JOIN dbo.[Batch] b ON b.FabricID = f.FabricID

GROUP BY 
    f.FabricID, f.FabricType, f.Composition, f.Color, 
    f.WeightPerUnit, f.StockQuantity, f.Description;
GO