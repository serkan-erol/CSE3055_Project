CREATE DATABASE KismetDB;
GO
USE KismetDB;
GO

--------------------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------ TABLES ------------------------------------------------------------
--------------------------------------------------------------------------------------------------------------------------------

-- User Super-Type --
CREATE TABLE dbo.[User] (
    UserID              int IDENTITY PRIMARY KEY,
    UserName            nvarchar(100) NOT NULL,
    -- For now, only email is necessary but it can be modified to allow sign-ups with either email or phone
    ContactEmail        nvarchar(100) UNIQUE NOT NULL, 
    ContactPhone        nvarchar(20) NULL,
    PasswordHash        nvarchar(255) NOT NULL,
    UserType            char(8) NOT NULL CHECK (UserType IN ('Customer', 'Employee')),
    CreatedAt           datetime2 NOT NULL DEFAULT sysdatetime(),
    LastUpdatedAt       datetime2 NULL
);

-- Session Table --
CREATE TABLE dbo.[Session] (
    SessionID             int IDENTITY ,
    UserID                int NOT NULL,
    AccessToken           nvarchar(500) NULL,
    RefreshToken          nvarchar(500) NOT NULL,
    ATExpiresAt           datetime2 NULL,
    RTExpiresAt           datetime2 NOT NULL,
    CreatedAt             datetime2 NOT NULL DEFAULT sysdatetime(),
    LastUpdatedAt         datetime2 NULL,

    CONSTRAINT FK_Session_User
        FOREIGN KEY (UserID) REFERENCES dbo.[User](UserID)
);

--Clustered index on CreatedAt reverse
CREATE CLUSTERED INDEX IX_Session_CreatedAt_DESC ON dbo.[Session] (CreatedAt DESC);

-- Nonclustered indexes for UserID and SessionID
CREATE NONCLUSTERED INDEX IX_Session_UserID ON dbo.[Session] (UserID);
CREATE NONCLUSTERED INDEX IX_Session_SessionID ON dbo.[Session] (SessionID);

-- Unique index to support the foreing key references for the Employee and the Customer tables
CREATE UNIQUE INDEX UQ_User_UserID_UserType ON dbo.[User](UserID, UserType);

-- Employee Sub-Type --
CREATE TABLE dbo.[Employee] (
    EmployeeID          int PRIMARY KEY,
    UserType            AS CAST('Employee' AS char(8)) PERSISTED,
    EmployeeNumber      char(10) NOT NULL DEFAULT 'E000000000',
    EmployeeRole        nvarchar(50) NOT NULL,
    AccessLevel         int NOT NULL CHECK (AccessLevel >= 0 AND AccessLevel <= 10),

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
    ReliabilityStatus   bit NOT NULL DEFAULT 1, -- 1 = Reliable /// 0 = Unreliable
    City                nvarchar(50) NULL,
    Country             nvarchar(50) NULL,

    CONSTRAINT FK_Customer_User_Subtype
        FOREIGN KEY (CustomerID, UserType) REFERENCES dbo.[User](UserID, UserType)
);

-- Saved Payment Methods of Customers for when they order from us --
CREATE TABLE dbo.[SavedPaymentMethod] (
    SPMID                int IDENTITY PRIMARY KEY,
    CustomerID           int NOT NULL,
    -- Result of an encoding process with a special key can be recorded in CardNumber for security purposes
    CardNumber           nvarchar(255) NOT NULL UNIQUE,
    CardType             nvarchar(20) NOT NULL CHECK (CardType IN ('Debit', 'Credit')),
    CardExpirationDate   date NOT NULL,
    RecordExpirationDate date NULL DEFAULT DATEADD(year, 5, GETDATE()),
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
    IBAN            nvarchar(50) NULL UNIQUE,
    CreatedAt       datetime2 NOT NULL DEFAULT sysdatetime(),
    LastUpdatedAt   datetime2 NULL,

    CONSTRAINT FK_SBI_Customer
        FOREIGN KEY (CustomerID) REFERENCES dbo.[Customer](CustomerID)
);

--------------------------------------------------------------------------------------------------------------------------------

-- Billing --
CREATE TABLE dbo.[Billing] (
    BillingID        int IDENTITY PRIMARY KEY,
    CustomerID       int NOT NULL,
    -- 'Purchase' for the customer buying from us and 'Supply' for us buying from the customer
    BillingType      nvarchar(8) NOT NULL CHECK (BillingType IN ('Purchase', 'Supply')),
    InvoiceNumber    nvarchar(50) NOT NULL UNIQUE,
    TotalDue         decimal(18, 2) NOT NULL CHECK (TotalDue > 0),
    TotalPaid        decimal(18, 2) NOT NULL DEFAULT 0.00 CHECK (TotalPaid >= 0),
    RemainingBalance AS (TotalDue - TotalPaid) PERSISTED CHECK (RemainingBalance >= 0),
    PaymentTerms     nvarchar(255) NULL,
    -- BillingDate is set to 6 months from the current date by default
    -- We assume BillingDate is the last day for the payment of the billing
    BillingDate      date NOT NULL DEFAULT DATEADD(month, 6, CAST(getdate() as date)),
    -- 0 = Unpaid, 1 = Partial, 2 = Paid
    BillingStatus    int NOT NULL DEFAULT 0 CHECK(BillingStatus BETWEEN 0 AND 2),
    CreatedAt        datetime2 NOT NULL DEFAULT sysdatetime(),
    LastUpdatedAt    datetime2 NULL,

    CONSTRAINT FK_Billing_Customer
        FOREIGN KEY (CustomerID) REFERENCES dbo.[Customer](CustomerID)
);

--------------------------------------------------------------------------------------------------------------------------------

-- Order --
CREATE TABLE dbo.[Order] (
    OrderID         int IDENTITY PRIMARY KEY,
    CustomerID      int NOT NULL,
    OrderNumber     nvarchar(50) NOT NULL DEFAULT 'Order00000',
    -- 'Purchase' for the customer buying from us and 'Supply' for us buying from the customer
    OrderType       nvarchar(8) NOT NULL CHECK (OrderType IN ('Purchase', 'Supply')),
    -- TotalAmount is 0 by default until the order is approved and the batches are created
    TotalAmount     decimal(18, 2) NOT NULL CHECK (TotalAmount >= 0),
    -- 0 = Pending, 1 = Approved, 2 = Shipped, 3 = Delivered, and 4 = Cancelled
    OrderStatus     int NOT NULL DEFAULT 0 CHECK (OrderStatus BETWEEN 0 AND 4),
    IsApproved      bit NOT NULL DEFAULT 0,
    ApprovedBy      int NULL,
    ApprovalDate    datetime2 NULL,
    IsLocked        bit NOT NULL DEFAULT 0,
    LockedAt        datetime2 NULL,
    OrderDate       datetime2 NOT NULL DEFAULT sysdatetime(),
    LastUpdatedAt   datetime2 NULL,

    CONSTRAINT FK_Order_Customer
        FOREIGN KEY (CustomerID) REFERENCES dbo.[Customer](CustomerID),
    CONSTRAINT FK_Order_Employee
        FOREIGN KEY (ApprovedBy) REFERENCES dbo.[Employee](EmployeeID)
);

--------------------------------------------------------------------------------------------------------------------------------

-- FinancialTransaction --
CREATE TABLE dbo.[FinancialTransaction] (
    FTransactionID      int IDENTITY PRIMARY KEY,
    CustomerID          int NOT NULL,
    BillingID           int NOT NULL,
    OrderID             int NOT NULL,
    -- 'Purchase' for the customer paying us and 'Supply' for us paying the customer
    TransactionType     nvarchar(8) NOT NULL CHECK (TransactionType IN ('Purchase', 'Supply')),
    TotalAmount         decimal(18, 2) NOT NULL CHECK (TotalAmount > 0),
    TotalPaid           decimal(18, 2) NOT NULL DEFAULT 0.00 CHECK (TotalPaid >= 0),
    RemainingBalance    AS (TotalAmount - TotalPaid) PERSISTED CHECK (RemainingBalance >= 0),
    -- 0 = Unpaid, 1 = Partial, 2 = Paid
    PaymentStatus       int NOT NULL DEFAULT 0 CHECK (PaymentStatus BETWEEN 0 AND 2), 
    Description         nvarchar(255) NULL,
    -- TransactionDate is set to 6 months from the current date by default
    -- We assume TransactionDate is the last day for the payment of the transaction
    TransactionDate     date NOT NULL DEFAULT DATEADD(month, 6, CAST(getdate() as date)),
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
    FTransactionID  int NULL,
    Amount          decimal(18, 2) NOT NULL,
    BalanceAfter    decimal(18, 2) NOT NULL,
    Description     nvarchar(255) NULL,
    EntryDate       datetime2 NOT NULL DEFAULT sysdatetime(),
    LastUpdatedAt   datetime2 NULL,

    CONSTRAINT FK_Treasury_Transaction
        FOREIGN KEY (FTransactionID) REFERENCES dbo.[FinancialTransaction](FTransactionID)
);

-- Insert the starting balance of the treasury
INSERT INTO dbo.[Treasury] (FTransactionID, Amount, BalanceAfter, Description)
VALUES (NULL, 0, 0, 'Starting balance');

-- Payment --
CREATE TABLE dbo.[Payment] (
    PaymentID        int IDENTITY PRIMARY KEY,
    FTransactionID   int NOT NULL,
    PaymentAmount    decimal(18, 2) NOT NULL CHECK (PaymentAmount > 0),
    -- 'Purchase' for the customer paying us (associated with a Purchase type Order / FinancialTransaction) 
    -- and 'Supply' for us paying the customer (associated with a Supply type Order / FinancialTransaction)
    PaymentType      nvarchar(8) NOT NULL CHECK (PaymentType IN ('Purchase', 'Supply', 'Refund')),
    PaymentDate      datetime2 NOT NULL DEFAULT sysdatetime(),
    PaymentMethod    nvarchar(50) NULL,     -- Cash, credit card, debit card etc.
    ReferenceNumber  nvarchar(100) NULL,    -- We can use a function and a trigger to generate a reference number within set parameters

    CONSTRAINT FK_Payment_Transaction
        FOREIGN KEY (FTransactionID) REFERENCES dbo.[FinancialTransaction](FTransactionID)
);

--------------------------------------------------------------------------------------------------------------------------------

-- Shipment --
CREATE TABLE dbo.[Shipment] (
    ShipmentID           int IDENTITY PRIMARY KEY,
    OrderID              int NOT NULL,
    CustomsDocRef        nvarchar(100) NOT NULL,
    -- 0 = Pending, 1 = In Transit, 2 = Delivered, and 3 = Failed
    ShipmentStatus       int NOT NULL DEFAULT 0 CHECK (ShipmentStatus BETWEEN 0 AND 3),
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
        FOREIGN KEY (OrderID) REFERENCES dbo.[Order](OrderID)
);

--------------------------------------------------------------------------------------------------------------------------------

-- Fabric --
CREATE TABLE dbo.[Fabric] (
    FabricID        int IDENTITY PRIMARY KEY,
    FabricType      nvarchar(50) NOT NULL,
    Composition     nvarchar(100) NULL,
    Color           nvarchar(50) NULL,
    WeightPerUnit   decimal(18, 2) NULL CHECK (WeightPerUnit > 0),
    -- Stock unit is always rolls. It is assumed all the roles have the same length and width
    StockQuantity   int NOT NULL DEFAULT 0,
    UnitPrice       decimal(18, 2) NOT NULL CHECK (UnitPrice > 0),
    Description     nvarchar(255) NULL
);

-- Batch --
CREATE TABLE dbo.[Batch] (
    BatchID         int IDENTITY PRIMARY KEY,
    OrderID         int NOT NULL,
    ShipmentID      int NULL,
    FabricID        int NOT NULL,
    BatchNumber     nvarchar(50) NOT NULL UNIQUE,
    Quantity        int NOT NULL,
    BatchPrice      decimal(18, 2) NOT NULL,
    ProductionDate  date NULL,
    QualityGrade    nvarchar(50) NULL,
    CreatedAt       datetime2 NOT NULL DEFAULT sysdatetime(),  

    CONSTRAINT FK_Batch_Order
        FOREIGN KEY (OrderID) REFERENCES dbo.[Order](OrderID),
    ---CONSTRAINT FK_Batch_Shipment
       --- FOREIGN KEY (ShipmentID) REFERENCES dbo.[Shipment](ShipmentID),
    CONSTRAINT FK_Batch_Fabric
        FOREIGN KEY (FabricID) REFERENCES dbo.[Fabric](FabricID)
);

--------------------------------------------------------------------------------------------------------------------------------
----------------------------------------------------------- INDEXES -----------------------------------------------------------
--------------------------------------------------------------------------------------------------------------------------------

CREATE INDEX IDX_User_ContactEmail ON dbo.[User](ContactEmail);

CREATE INDEX IDX_Session_AccessToken_ATExpiresAt ON dbo.[Session](AccessToken, ATExpiresAt);
CREATE INDEX IDX_Session_RefreshToken_RTExpiresAt ON dbo.[Session](RefreshToken, RTExpiresAt);
CREATE INDEX IDX_Session_LastUpdatedAt ON dbo.[Session](LastUpdatedAt);

CREATE INDEX IDX_Employee_EmployeeNumber ON dbo.[Employee](EmployeeNumber);
CREATE INDEX IDX_Employee_EmployeeRole ON dbo.[Employee](EmployeeRole);
CREATE INDEX IDX_Employee_AccessLevel ON dbo.[Employee](AccessLevel);

CREATE INDEX IDX_Customer_CustomerNumber ON dbo.[Customer](CustomerNumber);

CREATE INDEX IDX_SavedPaymentMethod_CustomerID ON dbo.[SavedPaymentMethod](CustomerID);
CREATE INDEX IDX_SavedPaymentMethod_CardNumber_CardExpirationDate ON dbo.[SavedPaymentMethod](CardNumber, CardExpirationDate);

CREATE INDEX IDX_SavedBankInformation_CustomerID ON dbo.[SavedBankInformation](CustomerID);
CREATE INDEX IDX_SavedBankInformation_IBAN ON dbo.[SavedBankInformation](IBAN);

CREATE INDEX IDX_Billing_CustomerID_BillingType ON dbo.[Billing](CustomerID, BillingType);
CREATE INDEX IDX_Billing_CustomerID_RemainingBalance ON dbo.[Billing](CustomerID, RemainingBalance);
CREATE INDEX IDX_Billing_CustomerID_BillingStatus ON dbo.[Billing](CustomerID, BillingStatus);
CREATE INDEX IDX_Billing_InvoiceNumber ON dbo.[Billing](InvoiceNumber);

CREATE INDEX IDX_Order_OrderNumber ON dbo.[Order](OrderNumber);
CREATE INDEX IDX_Order_CustomerID_OrderType ON dbo.[Order](CustomerID, OrderType);
CREATE INDEX IDX_Order_CustomerID_OrderStatus ON dbo.[Order](CustomerID, OrderStatus);
CREATE INDEX IDX_Order_TotalAmount ON dbo.[Order](TotalAmount);
CREATE INDEX IDX_Order_IsApproved_ApprovedBy ON dbo.[Order](IsApproved, ApprovedBy);
CREATE INDEX IDX_Order_OrderStatus_IsLocked_LockedAt ON dbo.[Order](OrderStatus, IsLocked, LockedAt);
CREATE INDEX IDX_Order_OrderDate ON dbo.[Order](OrderDate);
CREATE INDEX IDX_Order_LastUpdatedAt ON dbo.[Order](LastUpdatedAt);

CREATE INDEX IDX_FT_BillingID_TransactionType ON dbo.[FinancialTransaction](BillingID, TransactionType);
CREATE INDEX IDX_FT_PaymentStatus ON dbo.[FinancialTransaction](PaymentStatus);
CREATE INDEX IDX_FT_TransactionDate ON dbo.[FinancialTransaction](TransactionDate);

CREATE INDEX IDX_Treasury_FTransactionID ON dbo.[Treasury](FTransactionID);
CREATE INDEX IDX_Treasury_Amount_BalanceAfter ON dbo.[Treasury](Amount, BalanceAfter);
CREATE INDEX IDX_Treasury_EntryDate ON dbo.[Treasury](EntryDate);
CREATE INDEX IDX_Treasury_LastUpdatedAt ON dbo.[Treasury](LastUpdatedAt);

CREATE INDEX IDX_Payment_FTransactionID_PaymentType ON dbo.[Payment](FTransactionID, PaymentType);
CREATE INDEX IDX_Payment_PaymentAmount_PaymentDate ON dbo.[Payment](PaymentAmount, PaymentDate);
CREATE INDEX IDX_Payment_ReferenceNumber ON dbo.[Payment](ReferenceNumber);

CREATE INDEX IDX_Shipment_OrderID_ShipmentStatus ON dbo.[Shipment](OrderID, ShipmentStatus);
CREATE INDEX IDX_Shipment_CustomsDocRef ON dbo.[Shipment](CustomsDocRef);
CREATE INDEX IDX_Shipment_ShipmentStatus_IsLocked_LockedAt ON dbo.[Shipment](ShipmentStatus, IsLocked, LockedAt);
CREATE INDEX IDX_Shipment_ShipmentDate ON dbo.[Shipment](ShipmentDate);
CREATE INDEX IDX_Shipment_OriginCountry_DestinationCountry ON dbo.[Shipment](OriginCountry, DestinationCountry);
CREATE INDEX IDX_Shipment_ExpectedDeliveryDate ON dbo.[Shipment](ExpectedDeliveryDate);
CREATE INDEX IDX_Shipment_ExpectedDeliveryDate_ActualDeliveryDate ON dbo.[Shipment](ExpectedDeliveryDate, ActualDeliveryDate);
CREATE INDEX IDX_Shipment_LastUpdatedAt ON dbo.[Shipment](LastUpdatedAt);

CREATE INDEX IDX_Fabric_FabricType_Composition ON dbo.[Fabric](FabricType, Composition);
CREATE INDEX IDX_Fabric_StockQuantity ON dbo.[Fabric](StockQuantity);
CREATE INDEX IDX_Fabric_UnitPrice ON dbo.[Fabric](UnitPrice);

CREATE INDEX IDX_Batch_OrderID_ShipmentID ON dbo.[Batch](OrderID, ShipmentID);
CREATE INDEX IDX_Batch_FabricID ON dbo.[Batch](FabricID);
CREATE INDEX IDX_Batch_BatchNumber ON dbo.[Batch](BatchNumber);
CREATE INDEX IDX_Batch_FabricID_Quantity_BatchPrice ON dbo.[Batch](FabricID, Quantity, BatchPrice);
CREATE INDEX IDX_Batch_CreatedAt ON dbo.[Batch](CreatedAt);