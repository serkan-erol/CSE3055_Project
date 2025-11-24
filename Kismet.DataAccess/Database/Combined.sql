-- Customer --
CREATE TABLE dbo.[Customer](
    CustomerID       int IDENTITY PRIMARY KEY,
    CustomerName     nvarchar(100) NOT NULL,
    CustomerType     nvarchar(50)  NULL,
    ReliabilityStatus nvarchar(50) NULL,
    PaymentType      nvarchar(50)  NULL,
    ContactInfo      nvarchar(200) NULL
);

-- Order --
CREATE TABLE dbo.[Order] (
    OrderID       int IDENTITY PRIMARY KEY,
    CustomerID    int NOT NULL,
    OrderDate     datetime2 NOT NULL DEFAULT sysdatetime(),
    TotalAmount   decimal(18,2) NOT NULL,
    OrderStatus   nvarchar(50) NULL,
    ApprovedBy    nvarchar(100) NULL,
    ApprovalDate  datetime2 NULL,

    CONSTRAINT FK_Order_Customer
        FOREIGN KEY (CustomerID) REFERENCES dbo.[Customer](CustomerID)
);

-- FinancialTransaction --
CREATE TABLE dbo.[FinancialTransaction] (
    FinancialTransactionID   int IDENTITY PRIMARY KEY,
    CustomerID      int NOT NULL,
    OrderID         int NOT NULL,
    TransactionType nvarchar(50) NOT NULL,
    TransactionDate datetime2 NOT NULL DEFAULT sysdatetime(),
    TotalAmount     decimal(18,2) NOT NULL,
    PaymentStatus   nvarchar(50) NULL,
    Description     nvarchar(200) NULL,

    CONSTRAINT FK_Transaction_Customer
        FOREIGN KEY (CustomerID) REFERENCES dbo.[Customer](CustomerID),
        FOREIGN KEY (OrderID) REFERENCES dbo.[Order](OrderID)
);

-- Treasury --
CREATE TABLE dbo.[Treasury] (
    TreasuryID   int IDENTITY PRIMARY KEY,
    FinancialTransactionID  int NOT NULL,
    EntryDate    datetime2 NOT NULL DEFAULT sysdatetime(),
    Amount       decimal(18,2) NOT NULL,
    BalanceAfter decimal(18,2) NULL,
    Description  nvarchar(200) NULL,

    CONSTRAINT FK_Treasury_Transaction
        FOREIGN KEY (FinancialTransactionID) REFERENCES dbo.[FinancialTransaction](FinancialTransactionID)
);

-- Payment --
CREATE TABLE dbo.[Payment] (
    PaymentID        int IDENTITY PRIMARY KEY,
    FinancialTransactionID    int NOT NULL,
    BillingID        int NULL,
    PaymentAmount    decimal(18,2) NOT NULL,
    PaymentDate      datetime2 NOT NULL DEFAULT sysdatetime(),
    PaymentMethod    nvarchar(50) NULL,
    ReferenceNumber  nvarchar(100) NULL,  -- We can use a trigger to generate a reference number within set parameters

    CONSTRAINT FK_Payment_Transaction
        FOREIGN KEY (FinancialTransactionID) REFERENCES dbo.[FinancialTransaction](FinancialTransactionID)
);

-- Billing --
CREATE TABLE dbo.[Billing] (
    BillingID        int IDENTITY PRIMARY KEY,
    CustomerID       int NOT NULL,
    OrderID          int NOT NULL,
    InvoiceNumber    nvarchar(50) NOT NULL UNIQUE,
    TotalDue         decimal(18,2) NOT NULL,
    TotalPaid        decimal(18,2) NOT NULL DEFAULT 0,
    RemainingBalance AS (TotalDue - TotalPaid) PERSISTED,
    PaymentTerms     nvarchar(100) NULL,
    BillingDate      date NOT NULL DEFAULT cast(getdate() as date),
    Status           nvarchar(50) NULL,

    CONSTRAINT FK_Billing_Order
        FOREIGN KEY (OrderID) REFERENCES dbo.[Order](OrderID),
    CONSTRAINT FK_Billing_Customer
        FOREIGN KEY (CustomerID) REFERENCES dbo.[Customer](CustomerID)
);

GO

CREATE OR ALTER TRIGGER dbo.TR_Billing_ValidateCustomer
ON dbo.[Billing]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.[Order] o ON o.OrderID = i.OrderID
        WHERE o.CustomerID <> i.CustomerID
    )
    BEGIN
        RAISERROR ('Billing.CustomerID must match the customer who placed the order.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;

GO

ALTER TABLE dbo.[Payment]
    WITH CHECK ADD CONSTRAINT FK_Payment_Billing
    FOREIGN KEY (BillingID) REFERENCES dbo.[Billing](BillingID);

GO

-- Shipment --
CREATE TABLE dbo.[Shipment] (
    ShipmentID          int IDENTITY PRIMARY KEY,
    OrderID             int NOT NULL,
    CustomsDocRef       nvarchar(100) NULL,
    ShipmentStatus      nvarchar(50) NULL,
    ShipmentDate        date NULL,
    OriginCountry       nvarchar(50) NULL,
    DestinationCountry  nvarchar(50) NULL,
    ExpectedDeliveryDate date NULL,
    ActualDeliveryDate   date NULL,

    CONSTRAINT FK_Shipment_Order
        FOREIGN KEY (OrderID) REFERENCES dbo.[Order](OrderID)
);

-- Fabric --
CREATE TABLE dbo.[Fabric] (
    FabricID       int IDENTITY PRIMARY KEY,
    FabricType     nvarchar(50) NOT NULL,
    Composition    nvarchar(100) NULL,
    Color          nvarchar(50) NULL,
    WeightPerUnit  decimal(10,2) NULL,
    StockQuantity  int NOT NULL DEFAULT 0,
    Unit           nvarchar(20) NULL
);

-- Batch --
CREATE TABLE dbo.[Batch] (
    BatchID        int IDENTITY PRIMARY KEY,
    FabricID       int NOT NULL,
    ShipmentID     int NOT NULL,
    BatchNumber    nvarchar(50) NOT NULL UNIQUE,
    Quantity       int NOT NULL,
    ProductionDate date NULL,
    QualityGrade   nvarchar(50) NULL,  

    CONSTRAINT FK_Batch_Fabric
        FOREIGN KEY (FabricID) REFERENCES dbo.[Fabric](FabricID),
    CONSTRAINT FK_Batch_Shipment
        FOREIGN KEY (ShipmentID) REFERENCES dbo.[Shipment](ShipmentID)
);