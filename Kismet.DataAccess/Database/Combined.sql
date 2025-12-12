--------------------------------------------------------------------------------------------------------------------------------------
--------------------------------------------------------------- TABLES ---------------------------------------------------------------
--------------------------------------------------------------------------------------------------------------------------------------

-- User Super-Type --
CREATE TABLE dbo.[User] (
    UserID              int IDENTITY PRIMARY KEY,
    UserName            nvarchar(100) NOT NULL,
    -- For now, only email is necessary but it can be modified to allow sign-ups with either email or phone
    ContactEmail        nvarchar(100) UNIQUE NOT NULL, 
    ContactPhone        nvarchar(20) UNIQUE NULL,
    PasswordHash        nvarchar(255) NOT NULL,
    UserType            char(8) NOT NULL CHECK (UserType IN ('Customer', 'Employee')),
    CreatedAt           datetime2 NOT NULL DEFAULT sysdatetime(),
    LastUpdatedAt       datetime2 NULL
);

-- Unique index to support the foreing key references for the Employee and the Customer tables
CREATE UNIQUE INDEX UQ_User_UserID_UserType ON dbo.[User](UserID, UserType);

-- Employee Sub-Type --
CREATE TABLE dbo.[Employee] (
    EmployeeID          int PRIMARY KEY,
    UserType            AS CAST('Employee' AS char(8)) PERSISTED,
    EmployeeNumber      char(10) NOT NULL DEFAULT 'E000000000',
    EmployeeRole        nvarchar(50) NOT NULL,
    AccessLevel         int NOT NULL,

    CONSTRAINT FK_Employee_User_Subtype
        FOREIGN KEY (EmployeeID, UserType) REFERENCES dbo.[User](UserID, UserType)
);

-- Customer Sub-Type --
CREATE TABLE dbo.[Customer] (
    CustomerID          int PRIMARY KEY,
    UserType            AS CAST('Customer' AS char(8)) PERSISTED,
    CustomerNumber      char(10) NOT NULL DEFAULT 'C000000000',
    CustomerType        nvarchar(50) NULL,
    -- Created as a reliable customer. Stays that way unless updated manually by employees.
    ReliabilityStatus  bit NOT NULL DEFAULT 1, -- 1 = Reliable /// 0 = Unreliable

    CONSTRAINT FK_Customer_User_Subtype
        FOREIGN KEY (CustomerID, UserType) REFERENCES dbo.[User](UserID, UserType)
);

-- Saved Payment Methods of Customers for when they order from us --
CREATE TABLE dbo.[SavedPaymentMethod] (
    SPMID                int IDENTITY PRIMARY KEY,
    CustomerID           int NOT NULL,
    -- Result of an encoding process with a special key can be recorded in CardNumber for security purposes
    CardNumber           nvarchar(255) NOT NULL,
    CardType             nvarchar(20) NOT NULL CHECK (CardType IN ('Debit', 'Credit')),
    CardExpirationDate   date NOT NULL,
    RecordExpirationDate date NULL,
    CreatedAt            datetime2 NOT NULL DEFAULT sysdatetime(),
    LastUpdatedAt        datetime2 NULL,

    CONSTRAINT FK_SavedPaymentMethod_Customer
        FOREIGN KEY (CustomerID) REFERENCES dbo.[Customer](CustomerID)
);

-- Saved Bank Information of Customers for when we supply Fabric from them --
CREATE TABLE dbo.[SavedBankInformation] (
    SBIID           int IDENTITY PRIMARY KEY,
    CustomerID      int NOT NULL,
    BankName        nvarchar(50) NULL,
    AccountNo       nvarchar(50) NULL,
    IBAN            nvarchar(50) NULL,
    CreatedAt       datetime2 NOT NULL DEFAULT sysdatetime(),
    LastUpdatedAt   datetime2 NULL,

    CONSTRAINT FK_SBI_Customer
        FOREIGN KEY (CustomerID) REFERENCES dbo.[Customer](CustomerID)
);

-- Billing --
CREATE TABLE dbo.[Billing] (
    BillingID        int IDENTITY PRIMARY KEY,
    CustomerID       int NOT NULL,
    InvoiceNumber    nvarchar(50) NOT NULL UNIQUE,
    TotalDue         decimal(18, 2) NOT NULL,
    TotalPaid        decimal(18, 2) NOT NULL DEFAULT 0.00,
    RemainingBalance AS (TotalDue - TotalPaid) PERSISTED,
    PaymentTerms     nvarchar(255) NULL,
    BillingDate      date NOT NULL DEFAULT CAST(getdate() as date),
    -- 0 = Unpaid, 1 = Partial, 2 = Paid
    BillingStatus    int NOT NULL DEFAULT 0 CHECK(BillingStatus IN (0,2)),
    CreatedAt        datetime2 NOT NULL DEFAULT sysdatetime(),
    LastUpdatedAt    datetime2 NULL,

    CONSTRAINT FK_Billing_Customer
        FOREIGN KEY (CustomerID) REFERENCES dbo.[Customer](CustomerID)
);

-- Order Super-Type --
CREATE TABLE dbo.[Order] (
    OrderID        int IDENTITY PRIMARY KEY,
    CustomerID     int NOT NULL,
    OrderDate      date NOT NULL DEFAULT CAST(getdate() as date),
    -- 0 = Pending, 1 = Approved, 2 = Shipped, 3 = Delivered, and 4 = Cancelled
    OrderStatus    int NOT NULL DEFAULT 0 CHECK (OrderStatus IN (0, 4)),
    IsLocked       bit NOT NULL DEFAULT 0,
    LockedAt       datetime2 NULL,
    OrderType      nvarchar(8) NOT NULL CHECK (OrderType IN ('Purchase', 'Supply')),
    CreatedAt      datetime2 NOT NULL DEFAULT sysdatetime(),
    LastUpdatedAt  datetime2 NULL,

    CONSTRAINT FK_Order_Customer
        FOREIGN KEY (CustomerID) REFERENCES dbo.[Customer](CustomerID)
);

-- Unique index to support the foreing key references for the Supply / Purchase Order tables
CREATE UNIQUE INDEX UQ_Order_OrderID_OrderType ON dbo.[Order](OrderID, OrderType);

-- Supply Order Sub-Type --
CREATE TABLE dbo.[SupplyOrder] (
    SOrderID        int PRIMARY KEY,
    OrderType       AS CAST('Supply' AS nvarchar(8)) PERSISTED,
    AmountOwed      decimal(18, 2) NOT NULL,

    CONSTRAINT FK_SupplyOrder_Order_Subtype
        FOREIGN KEY (SOrderID, OrderType) REFERENCES dbo.[Order](OrderID, OrderType)
);

-- Purchase Order Sub-Type --
CREATE TABLE dbo.[PurchaseOrder] (
    POrderID        int PRIMARY KEY,
    OrderType       AS CAST('Purchase' AS nvarchar(8)) PERSISTED,
    TotalAmount     decimal(18, 2) NOT NULL,
    IsApproved      bit NOT NULL DEFAULT 0,
    ApprovedBy      int NULL,
    ApprovalDate    datetime2 NULL,

    CONSTRAINT FK_PurchaseOrder_Order_Subtype
        FOREIGN KEY (POrderID, OrderType) REFERENCES dbo.[Order](OrderID, OrderType)
);

-- FinancialTransaction --
CREATE TABLE dbo.[FinancialTransaction] (
    FTransactionID      int IDENTITY PRIMARY KEY,
    CustomerID          int NOT NULL,
    BillingID           int NOT NULL,
    OrderID             int NOT NULL,
    TransactionType     nvarchar(10) NOT NULL CHECK (TransactionType IN ('Puchase', 'Sale')),
    TotalAmount         decimal(18, 2) NOT NULL,
    TotalPaid           decimal(18, 2) NOT NULL DEFAULT 0.00,
    RemainingBalance    AS (TotalAmount - TotalPaid) PERSISTED,
    -- 0 = Unpaid, 1 = Partial, 2 = Paid
    PaymentStatus       int NOT NULL DEFAULT 0 CHECK (PaymentStatus IN (0, 2)), 
    Description         nvarchar(255) NULL,
    TransactionDate     datetime2 NOT NULL DEFAULT sysdatetime(),
    LastUpdatedAt       datetime2 NULL,

    CONSTRAINT FK_Transaction_Customer
        FOREIGN KEY (CustomerID) REFERENCES dbo.[Customer](CustomerID),
    CONSTRAINT FK_Transaction_Billing
        FOREIGN KEY (BillingID) REFERENCES dbo.[Billing](BillingID),
    CONSTRAINT FK_Transaction_Order
        FOREIGN KEY (OrderID) REFERENCES dbo.[Order](OrderID)
);

-- Treasury --
CREATE TABLE dbo.[Treasury] (
    TreasuryID      int IDENTITY PRIMARY KEY,
    FTransactionID  int NOT NULL,
    EntryDate       datetime2 NOT NULL DEFAULT sysdatetime(),
    Amount          decimal(18, 2) NOT NULL,
    BalanceAfter    decimal(18, 2) NOT NULL,
    Description     nvarchar(255) NULL,
    -- There should NOT be any updated in the rows of this table. This is to check if we are doing it right
    LastUpdatedAt   datetime2 NULL,

    CONSTRAINT FK_Treasury_Transaction
        FOREIGN KEY (FTransactionID) REFERENCES dbo.[FinancialTransaction](FTransactionID)
);

-- Payment --
CREATE TABLE dbo.[Payment] (
    PaymentID        int IDENTITY PRIMARY KEY,
    BillingID        int NOT NULL,
    FTransactionID   int NOT NULL,
    PaymentAmount    decimal(18, 2) NOT NULL,
    PaymentType      nvarchar(50) NOT NULL, -- Send money or Recieve depending on the Transaction and/or  related Order Type
    PaymentDate      datetime2 NOT NULL DEFAULT sysdatetime(),
    PaymentMethod    nvarchar(50) NULL,     -- Cash, credit card, debit card etc.
    ReferenceNumber  nvarchar(100) NULL,    -- We can use a function and a trigger to generate a reference number within set parameters

    CONSTRAINT FK_Payment_Billing
        FOREIGN KEY (BillingID) REFERENCES dbo.[Billing](BillingID),
    CONSTRAINT FK_Payment_Transaction
        FOREIGN KEY (FTransactionID) REFERENCES dbo.[FinancialTransaction](FTransactionID)
);

-- Shipment --
CREATE TABLE dbo.[Shipment] (
    ShipmentID           int IDENTITY PRIMARY KEY,
    POrderID             int NOT NULL,
    CustomsDocRef        nvarchar(100) NULL,
    -- 0 = Pending, 1 = In Transit, 2 = Delivered, and 3 = Failed
    ShipmentStatus       int NOT NULL DEFAULT 0 CHECK (ShipmentStatus IN (0, 3)),
    IsLocked             bit NOT NULL DEFAULT 0,
    LockedAt             datetime2 NULL,
    ShipmentDate         date NULL,
    OriginCountry        nvarchar(50) NULL,
    DestinationCountry   nvarchar(50) NULL,
    ExpectedDeliveryDate date NULL,
    ActualDeliveryDate   date NULL,
    CreatedAt            datetime2 NOT NULL DEFAULT sysdatetime(),
    LastUpdatedAt        datetime2 NULL,

    CONSTRAINT FK_Shipment_Order
        FOREIGN KEY (POrderID) REFERENCES dbo.[Order](OrderID)
);

-- Fabric --
CREATE TABLE dbo.[Fabric] (
    FabricID        int IDENTITY PRIMARY KEY,
    FabricType      nvarchar(50) NOT NULL,
    Composition     nvarchar(100) NULL,
    Color           nvarchar(50) NULL,
    WeightPerUnit   decimal(10,2) NULL,
    -- Stock unit is always rolls. It is assumed all the roles have the same length and width
    StockQuantity   int NOT NULL DEFAULT 0,
    Description     nvarchar(255) NULL
);

-- Unit Price for Fabric --
CREATE TABLE dbo.[UnitPrice] (
    UPID                int IDENTITY PRIMARY KEY,
    FabricID            int NOT NULL,
    Price               decimal(18, 2) NOT NULL,
    Currency            nvarchar(50) NOT NULL,
    EquivalentTLPrice   decimal(18, 2) NOT NULL,

    CONSTRAINT FK_UnitPrice_Fabric
        FOREIGN KEY (FabricID) REFERENCES dbo.[Fabric](FabricID)
);

-- Batch --
CREATE TABLE dbo.[Batch] (
    BatchID         int IDENTITY PRIMARY KEY,
    OrderID         int NOT NULL,
    ShipmentID      int NOT NULL,
    FabricID        int NOT NULL,
    BatchNumber     nvarchar(50) NOT NULL UNIQUE,
    Quantity        int NOT NULL,
    BatchPrice      decimal(18, 2) NOT NULL,
    ProductionDate  date NULL,
    QualityGrade    nvarchar(50) NULL,
    CreatedAt       datetime2 NOT NULL DEFAULT sysdatetime(),  

    CONSTRAINT FK_Batch_Order
        FOREIGN KEY (OrderID) REFERENCES dbo.[Order](OrderID),
    CONSTRAINT FK_Batch_Shipment
        FOREIGN KEY (ShipmentID) REFERENCES dbo.[Shipment](ShipmentID),
    CONSTRAINT FK_Batch_Fabric
        FOREIGN KEY (FabricID) REFERENCES dbo.[Fabric](FabricID)
);

--------------------------------------------------------------------------------------------------------------------------------------
-------------------------------------------------------- FUNCTIONS & TRIGGERS --------------------------------------------------------
--------------------------------------------------------------------------------------------------------------------------------------

-- Trigger to validate that the customer who placed the order matches the customer who is being billed --
CREATE OR ALTER TRIGGER dbo.TR_Billing_ValidateCustomer
ON dbo.[Billing]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.[FinancialTransaction] ft ON ft.BillingID = i.BillingID
        INNER JOIN dbo.[Order] o ON o.OrderID = ft.OrderID
        WHERE o.CustomerID <> i.CustomerID
    )
    BEGIN
        RAISERROR ('Billing.CustomerID must match the customer who placed the order.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;

-- A function and a trigger to check if the payment amount is NOT greater than the remaining debt (RemainingBalance in Billing table) --
CREATE OR ALTER FUNCTION dbo.FN_CheckPaymentAmount
(
    @PaymentAmount decimal(18, 2),
    @RemainingBalance decimal(18, 2)
)
RETURNS bit
AS
BEGIN
    RETURN CASE WHEN @PaymentAmount > @RemainingBalance THEN 0 ELSE 1 END;
END;

CREATE OR ALTER TRIGGER dbo.TR_Payment_CheckPaymentAmount
ON dbo.[Payment]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
END;
    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.[Billing] b ON b.BillingID = i.BillingID
        WHERE dbo.FN_CheckPaymentAmount(i.PaymentAmount, b.RemainingBalance) = 0
    )
    BEGIN
        RAISERROR ('Payment amount is greater than the remaining debt.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;

-- A function to make sure unreliable customers pay upfront and bill is paid completely before approving the order --
-- !!! --