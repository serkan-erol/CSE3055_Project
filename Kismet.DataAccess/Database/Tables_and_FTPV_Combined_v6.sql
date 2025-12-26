
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
    SessionID             int IDENTITY PRIMARY KEY,
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
    BillingID        int NOT NULL,
    FTransactionID   int NOT NULL,
    PaymentAmount    decimal(18, 2) NOT NULL CHECK (PaymentAmount > 0),
    -- 'Purchase' for the customer paying us (associated with a Purchase type Order / FinancialTransaction) 
    -- and 'Supply' for us paying the customer (associated with a Supply type Order / FinancialTransaction)
    PaymentType      nvarchar(8) NOT NULL CHECK (PaymentType IN ('Purchase', 'Supply', 'Refund')),
    PaymentDate      datetime2 NOT NULL DEFAULT sysdatetime(),
    PaymentMethod    nvarchar(50) NULL,     -- Cash, credit card, debit card etc.
    ReferenceNumber  nvarchar(100) NULL,    -- We can use a function and a trigger to generate a reference number within set parameters

    CONSTRAINT FK_Payment_Billing
        FOREIGN KEY (BillingID) REFERENCES dbo.[Billing](BillingID),
    CONSTRAINT FK_Payment_Transaction
        FOREIGN KEY (FTransactionID) REFERENCES dbo.[FinancialTransaction](FTransactionID)
);

--------------------------------------------------------------------------------------------------------------------------------

-- Shipment --
CREATE TABLE dbo.[Shipment] (
    ShipmentID           int IDENTITY PRIMARY KEY,
    OrderID              int NOT NULL,
    CustomsDocRef        nvarchar(100) NULL,
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

--------------------------------------------------------------------------------------------------------------------------------
---------------------------------------------------------- PROCEDURES ----------------------------------------------------------
--------------------------------------------------------------------------------------------------------------------------------

-- Procedure to assign a random and unique EmployeeNumber or CustomerNumber or OrderNumber based on the prefix
IF OBJECT_ID('dbo.AssignRandomNumber', 'P') IS NOT NULL
    DROP PROCEDURE dbo.AssignRandomNumber;
GO

CREATE PROCEDURE dbo.AssignRandomNumber
    @Prefix      char(1),      -- 'E' for Employee, 'C' for Customer, 'P' for Purchase Order, 'S' for Supply Order
    @Id          int,          -- EmployeeID or CustomerID or OrderID
    @MaxAttempts int = 100
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @GeneratedNumber   char(10);
    DECLARE @RandomDigits varchar(9);
    DECLARE @Exists       bit = 1;
    DECLARE @Attempts     int = 0;
    DECLARE @TableName    nvarchar(50);

    IF @Prefix NOT IN ('E', 'C', 'P', 'S')
    BEGIN
        RAISERROR('Invalid prefix. Must be ''E'' for Employee, ''C'' for Customer, ''P'' for Purchase Order, or ''S'' for Supply Order.', 16, 1);
        RETURN;
    END

    -- Set table name based on prefix
    SET @TableName = CASE WHEN @Prefix = 'E' THEN 'Employee' 
                          WHEN @Prefix = 'C' THEN 'Customer' 
                          WHEN @Prefix = 'P' OR @Prefix = 'S' THEN 'Order' END;

    -- Try until we find a unique number or hit the attempt limit
    WHILE @Exists = 1 AND @Attempts < @MaxAttempts
    BEGIN
        SET @Attempts += 1;

        -- 9 random digits
        SET @RandomDigits = RIGHT('000000000'
                                  + CAST(ABS(CHECKSUM(NEWID())) % 1000000000 AS varchar(9)), 9);
        -- 10 digit complete number
        SET @GeneratedNumber = @Prefix + @RandomDigits;

        IF @Prefix = 'E'
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.[Employee] WHERE EmployeeNumber = @GeneratedNumber)
                SET @Exists = 0;
        END
        ELSE IF @Prefix = 'C'
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.[Customer] WHERE CustomerNumber = @GeneratedNumber)
                SET @Exists = 0;
        END
        ELSE IF @Prefix = 'P' OR @Prefix = 'S'
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.[Order] WHERE OrderNumber = @GeneratedNumber)
                SET @Exists = 0;
        END
    END

    IF @Exists = 1
    BEGIN
        RAISERROR('Unable to generate unique %sNumber after %d attempts.', 16, 1, @TableName, @MaxAttempts);
        RETURN;
    END

    -- Persist the generated number on the correct table
    IF @Prefix = 'E'
    BEGIN
        UPDATE dbo.[Employee]
        SET EmployeeNumber = @GeneratedNumber
        WHERE EmployeeID = @Id;
    END
    ELSE IF @Prefix = 'C'
    BEGIN
        UPDATE dbo.[Customer]
        SET CustomerNumber = @GeneratedNumber
        WHERE CustomerID = @Id;
    END
    ELSE IF @Prefix = 'P' OR @Prefix = 'S'
    BEGIN
        UPDATE dbo.[Order]
        SET OrderNumber = @GeneratedNumber
        WHERE OrderID = @Id;
    END
END;
GO

--------------------------------------------------------------------------------------------------------------------------------

-- Procedure to update LastUpdatedAt timestamp for any table
CREATE PROCEDURE dbo.Update_LastUpdatedAt
    @Table nvarchar(50),
    @ID int,
    @IDColumn nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @SQL nvarchar(MAX);
    SET @SQL = N'UPDATE ' + QUOTENAME('dbo') + N'.' + QUOTENAME(@Table) + N' 
                 SET LastUpdatedAt = sysdatetime() 
                 WHERE ' + QUOTENAME(@IDColumn) + N' = @ID';
    
    EXEC sp_executesql @SQL, N'@ID int', @ID = @ID;
END;
GO

--------------------------------------------------------------------------------------------------------------------------------

-- Procedure to update IsLocked and LockedAt
CREATE PROCEDURE dbo.Update_IsLocked_LockedAt
    @Table nvarchar(50),
    @ID int,
    @IDColumn nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @SQL nvarchar(MAX);
    SET @SQL = N'UPDATE ' + QUOTENAME('dbo') + N'.' + QUOTENAME(@Table) + N' 
                 SET IsLocked = 1,
                     LockedAt = sysdatetime()
                 WHERE ' + QUOTENAME(@IDColumn) + N' = @ID';
    
    EXEC sp_executesql @SQL, N'@ID int', @ID = @ID;
END;
GO

--------------------------------------------------------------------------------------------------------------------------------

-- Procedure to check status change and update IsLocked/LockedAt if conditions are met
-- Used for Order and Shipment tables
CREATE PROCEDURE dbo.CheckStatusAndLock
    @Table nvarchar(50),
    @ID int,
    @IDColumn nvarchar(50),
    @StatusColumn nvarchar(50),
    @OldStatus int,
    @NewStatus int,
    @LockStatus1 int,
    @LockStatus2 int,
    @OldIsLocked bit
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if status changed to a locking status and record is not already locked
    IF (@NewStatus IN (@LockStatus1, @LockStatus2)
        AND (@OldIsLocked = 0 OR @OldIsLocked IS NULL))
    BEGIN
        EXEC dbo.Update_IsLocked_LockedAt @Table = @Table, @ID = @ID, @IDColumn = @IDColumn;
    END
END;
GO

--------------------------------------------------------------------------------------------------------------------------------

-- Procedure to update BillingStatus based on TotalDue and TotalPaid
CREATE PROCEDURE dbo.Update_BillingStatus
    @BillingID int
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.[Billing]
    SET BillingStatus = CASE
        WHEN TotalPaid = 0 THEN 0  -- Unpaid: No payments made yet
        WHEN TotalDue = TotalPaid THEN 2  -- Paid: Fully paid
        WHEN TotalDue > TotalPaid THEN 1  -- Partial: Some payment made but not fully paid
        ELSE -1 -- This should never happen! It is just a fallback to see if there are any erros in the logic
    END
    WHERE BillingID = @BillingID;
END;
GO

-- Procedure to update PaymentStatus of FT based on PaymentAmount and TotalPaid
CREATE PROCEDURE dbo.Update_FTPaymentStatus
    @FTransactionID int
AS
BEGIN
    SET NOCOUNT ON;
      UPDATE dbo.[FinancialTransaction]
    SET PaymentStatus = CASE
        WHEN TotalPaid = 0 THEN 0
        WHEN TotalAmount = TotalPaid THEN 2
        WHEN TotalAmount > TotalPaid THEN 1
        ELSE -1 -- This should never happen! It is just a fallback to see if there are any erros in the logic
    END
    WHERE FTransactionID = @FTransactionID;
END;
GO

--------------------------------------------------------------------------------------------------------------

-- Procedure to ship the placed orders
CREATE PROCEDURE dbo.ShipOrder
    @OrderID int,
    @EmployeeID int
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if shipments already exist for this order
    IF EXISTS (SELECT 1 FROM dbo.[Shipment] WHERE OrderID = @OrderID)
    BEGIN
        -- Return existing shipments
        SELECT 
            s.ShipmentID,
            s.ExpectedDeliveryDate,
            COUNT(b.BatchID) as BatchCount
        FROM dbo.[Shipment] s
        LEFT JOIN dbo.[Batch] b ON b.ShipmentID = s.ShipmentID
        WHERE s.OrderID = @OrderID
        GROUP BY s.ShipmentID, s.ExpectedDeliveryDate
        ORDER BY s.ShipmentID;
        RETURN;
    END

    -- Get all batches for this order
    DECLARE @Batches TABLE (BatchID int, RowNum int);
    INSERT INTO @Batches (BatchID, RowNum)
    SELECT BatchID, ROW_NUMBER() OVER (ORDER BY BatchID)
    FROM dbo.[Batch]
    WHERE OrderID = @OrderID;

    -- Check if there are any batches
    DECLARE @TotalBatches int = (SELECT COUNT(*) FROM @Batches);
    IF @TotalBatches = 0
    BEGIN
        RAISERROR('No batches found for this order', 16, 1);
        RETURN;
    END

    -- Calculate number of shipments needed (10 batches per shipment)
    DECLARE @MaxBatchesPerShipment int = 10;
    DECLARE @ShipmentsNeeded int = CEILING(@TotalBatches * 1.0 / @MaxBatchesPerShipment);
    DECLARE @CurrentShipment int = 1;
    DECLARE @ShipmentID int;
    DECLARE @ShipDate date = CAST(GETDATE() AS date);
    DECLARE @ExpectedDelivery date = DATEADD(day, 5, @ShipDate);

    -- Table to store created shipments for return
    DECLARE @CreatedShipments TABLE (
        ShipmentID int,
        ExpectedDeliveryDate date,
        BatchCount int
    );

    BEGIN TRANSACTION;
    BEGIN TRY
        -- Create shipments
        WHILE @CurrentShipment <= @ShipmentsNeeded
        BEGIN
            -- Calculate batch range for this shipment
            DECLARE @StartRow int = ((@CurrentShipment - 1) * @MaxBatchesPerShipment) + 1;
            DECLARE @EndRow int = @CurrentShipment * @MaxBatchesPerShipment;

            -- Create shipment record
            INSERT INTO dbo.[Shipment] (OrderID, ShipmentDate, ExpectedDeliveryDate)
            VALUES (@OrderID, @ShipDate, @ExpectedDelivery);
            
            SET @ShipmentID = SCOPE_IDENTITY();

            -- Link batches to this shipment
            UPDATE b
            SET b.ShipmentID = @ShipmentID
            FROM dbo.[Batch] b
            INNER JOIN @Batches bt ON bt.BatchID = b.BatchID
            WHERE bt.RowNum BETWEEN @StartRow AND @EndRow;

            -- Store shipment info for return
            INSERT INTO @CreatedShipments (ShipmentID, ExpectedDeliveryDate, BatchCount)
            SELECT 
                @ShipmentID,
                @ExpectedDelivery,
                COUNT(*)
            FROM @Batches
            WHERE RowNum BETWEEN @StartRow AND @EndRow;

            SET @CurrentShipment = @CurrentShipment + 1;
        END

        COMMIT TRANSACTION;

        -- Return created shipments
        SELECT 
            ShipmentID,
            ExpectedDeliveryDate,
            BatchCount
        FROM @CreatedShipments
        ORDER BY ShipmentID;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        THROW;
    END CATCH
END;
GO

--------------------------------------------------------------------------------------------------------------------------------

-- Create batches for multiple fabrics in an existing order
CREATE OR ALTER PROCEDURE dbo.CreateBatchesForMultipleFabrics
    @OrderID int,
    @FabricItemsJson nvarchar(MAX)  -- JSON array: [{"FabricID":1,"TotalFabricUnits":10,"QualityGrade":"A"},...]
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate OrderID exists
    IF NOT EXISTS (SELECT 1 FROM dbo.[Order] WHERE OrderID = @OrderID)
    BEGIN
        RAISERROR('Order with ID %d not found', 16, 1, @OrderID);
        RETURN;
    END

    -- Parse JSON and validate that at least one fabric item is provided
    IF @FabricItemsJson IS NULL OR LEN(LTRIM(RTRIM(@FabricItemsJson))) = 0
    BEGIN
        RAISERROR('Fabric items JSON must be provided', 16, 1);
        RETURN;
    END

    -- Create a temporary table to hold fabric items
    CREATE TABLE #FabricItems (
        FabricID int NOT NULL,
        TotalFabricUnits int NOT NULL,
        QualityGrade nvarchar(50) NULL
    );

    -- Parse JSON into temporary table
    BEGIN TRY
        INSERT INTO #FabricItems (FabricID, TotalFabricUnits, QualityGrade)
        SELECT 
            FabricID,
            TotalFabricUnits,
            QualityGrade
        FROM OPENJSON(@FabricItemsJson)
        WITH (
            FabricID int '$.FabricID',
            TotalFabricUnits int '$.TotalFabricUnits',
            QualityGrade nvarchar(50) '$.QualityGrade'
        );
    END TRY
    BEGIN CATCH
        DROP TABLE #FabricItems;
        RAISERROR('Invalid JSON format for fabric items', 16, 1);
        RETURN;
    END CATCH

    -- Validate that at least one fabric item is provided
    IF NOT EXISTS (SELECT 1 FROM #FabricItems)
    BEGIN
        DROP TABLE #FabricItems;
        RAISERROR('At least one fabric item must be provided', 16, 1);
        RETURN;
    END

    -- Get OrderType to determine if this is Purchase or Supply
    DECLARE @OrderType nvarchar(50);
    SELECT @OrderType = OrderType
    FROM dbo.[Order]
    WHERE OrderID = @OrderID;

    IF @OrderType IS NULL
    BEGIN
        DROP TABLE #FabricItems;
        RAISERROR('OrderType not found for OrderID %d', 16, 1, @OrderID);
        RETURN;
    END

    -- Validate all fabric items
    DECLARE @FabricID int;
    DECLARE @TotalFabricUnits int;
    DECLARE @CurrentStock int;
    DECLARE @QualityGrade nvarchar(50);

    DECLARE fabric_for_batch_cursor CURSOR FOR
        SELECT FabricID, TotalFabricUnits, QualityGrade
        FROM #FabricItems;

    OPEN fabric_for_batch_cursor;
    FETCH NEXT FROM fabric_for_batch_cursor INTO @FabricID, @TotalFabricUnits, @QualityGrade;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Validate quantity
        IF @TotalFabricUnits < 1
        BEGIN
            CLOSE fabric_for_batch_cursor;
            DEALLOCATE fabric_for_batch_cursor;
            DROP TABLE #FabricItems;
            RAISERROR('Quantity must be at least 1 for all fabric items', 16, 1);
            RETURN;
        END

        -- Check if fabric exists and get stock
        SELECT @CurrentStock = StockQuantity
        FROM dbo.[Fabric]
        WHERE FabricID = @FabricID;

        IF @CurrentStock IS NULL
        BEGIN
            CLOSE fabric_for_batch_cursor;
            DEALLOCATE fabric_for_batch_cursor;
            DROP TABLE #FabricItems;
            RAISERROR('Fabric with ID %d not found', 16, 1, @FabricID);
            RETURN;
        END

        -- Check stock availability ONLY for Purchase orders (we're selling fabric)
        -- For Supply orders, we're receiving fabric, so no stock check needed
        IF @OrderType = 'Purchase' AND @CurrentStock < @TotalFabricUnits
        BEGIN
            CLOSE fabric_for_batch_cursor;
            DEALLOCATE fabric_for_batch_cursor;
            DROP TABLE #FabricItems;
            RAISERROR('Insufficient fabric stock for FabricID %d. Available: %d, Requested: %d', 16, 1, @FabricID, @CurrentStock, @TotalFabricUnits);
            RETURN;
        END

        FETCH NEXT FROM fabric_for_batch_cursor INTO @FabricID, @TotalFabricUnits, @QualityGrade;
    END

    CLOSE fabric_for_batch_cursor;
    DEALLOCATE fabric_for_batch_cursor;

    BEGIN TRANSACTION;
    BEGIN TRY
        -- Process each fabric item: update stock and create batches
        DECLARE fabric_for_batch_cursor2 CURSOR FOR
            SELECT FabricID, TotalFabricUnits, QualityGrade
            FROM #FabricItems;

        OPEN fabric_for_batch_cursor2;
        FETCH NEXT FROM fabric_for_batch_cursor2 INTO @FabricID, @TotalFabricUnits, @QualityGrade;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- Update stock based on OrderType
            -- Purchase: Subtract stock (we're selling fabric to customer)
            -- Supply: Add stock (we're receiving fabric from customer)
            IF @OrderType = 'Purchase'
            BEGIN
                UPDATE dbo.[Fabric]
                SET StockQuantity = StockQuantity - @TotalFabricUnits
                WHERE FabricID = @FabricID;
            END
            ELSE IF @OrderType = 'Supply'
            BEGIN
                UPDATE dbo.[Fabric]
                SET StockQuantity = StockQuantity + @TotalFabricUnits
                WHERE FabricID = @FabricID;
            END

            -- Create batches for this fabric (trigger will update TotalAmount automatically)
            -- Note: We need to call CreateBatches without the "already batched" check
            -- So we'll create batches directly here
            DECLARE @UnitPrice decimal(18, 2);
            SELECT @UnitPrice = UnitPrice
            FROM dbo.[Fabric]
            WHERE FabricID = @FabricID;

            DECLARE @Remaining int = @TotalFabricUnits;
            DECLARE @BatchQty int;
            DECLARE @BatchPrice decimal(18,2);
            DECLARE @Counter int = 1;
            DECLARE @BatchNumber nvarchar(50);
            DECLARE @ProductionDate date = CAST(GETDATE() AS date);

            WHILE @Remaining > 0
            BEGIN
                SET @BatchQty = CASE WHEN @Remaining >= 20 THEN 20 ELSE @Remaining END;
                SET @BatchPrice = @BatchQty * @UnitPrice;

                -- Generate unique BatchNumber using timestamp + GUID + counter to ensure uniqueness
                -- Generate new GUID for each batch to guarantee uniqueness
                DECLARE @UniqueGuid nvarchar(36) = REPLACE(CAST(NEWID() AS nvarchar(36)), '-', '');
                SET @BatchNumber =
                    'BATCH-' + FORMAT(GETDATE(), 'yyyyMMddHHmmss') + '-' +
                    SUBSTRING(@UniqueGuid, 1, 8) + '-' +
                    RIGHT('000' + CAST(@Counter AS varchar(3)), 3);

                INSERT INTO dbo.[Batch] (
                    OrderID,
                    ShipmentID,
                    FabricID,
                    BatchNumber,
                    Quantity,
                    BatchPrice,
                    ProductionDate,
                    QualityGrade
                )
                VALUES (
                    @OrderID,
                    NULL,
                    @FabricID,
                    @BatchNumber,
                    @BatchQty,
                    @BatchPrice,
                    @ProductionDate,
                    @QualityGrade
                );

                SET @Remaining -= @BatchQty;
                SET @Counter += 1;
            END

            FETCH NEXT FROM fabric_for_batch_cursor2 INTO @FabricID, @TotalFabricUnits, @QualityGrade;
        END

        CLOSE fabric_for_batch_cursor2;
        DEALLOCATE fabric_for_batch_cursor2;

        DROP TABLE #FabricItems;

        COMMIT TRANSACTION;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        IF CURSOR_STATUS('global', 'fabric_for_batch_cursor2') >= 0
        BEGIN
            CLOSE fabric_for_batch_cursor2;
            DEALLOCATE fabric_for_batch_cursor2;
        END
        
        IF OBJECT_ID('tempdb..#FabricItems') IS NOT NULL
            DROP TABLE #FabricItems;
        
        THROW;
    END CATCH
END;
GO

--------------------------------------------------------------------------------------------------------------------------------
----------------------------------------------------------- TRIGGERS -----------------------------------------------------------
--------------------------------------------------------------------------------------------------------------------------------

-- Trigger to auto-generate random and unique EmployeeNumber on Employee insert
CREATE TRIGGER dbo.trg_GenerateEmployeeNumber
ON dbo.[Employee]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @EmployeeID int;

    DECLARE employee_cursor CURSOR FOR
        SELECT i.EmployeeID
        FROM inserted i
        INNER JOIN dbo.[Employee] e ON i.EmployeeID = e.EmployeeID
        WHERE e.EmployeeNumber = 'E000000000' OR e.EmployeeNumber IS NULL;

    OPEN employee_cursor;
    FETCH NEXT FROM employee_cursor INTO @EmployeeID;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC dbo.AssignRandomNumber @Prefix = 'E', @Id = @EmployeeID;
        FETCH NEXT FROM employee_cursor INTO @EmployeeID;
    END

    CLOSE employee_cursor;
    DEALLOCATE employee_cursor;
END;
GO

--------------------------------------------------------------------------------------------------------------------------------

-- Trigger to auto-generate random and unique CustomerNumber on Customer insert
CREATE TRIGGER dbo.trg_GenerateCustomerNumber
ON dbo.[Customer]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CustomerID int;

    DECLARE customer_cursor CURSOR FOR
        SELECT i.CustomerID
        FROM inserted i
        INNER JOIN dbo.[Customer] c ON i.CustomerID = c.CustomerID
        WHERE c.CustomerNumber = 'C000000000' OR c.CustomerNumber IS NULL;

    OPEN customer_cursor;
    FETCH NEXT FROM customer_cursor INTO @CustomerID;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC dbo.AssignRandomNumber @Prefix = 'C', @Id = @CustomerID;
        FETCH NEXT FROM customer_cursor INTO @CustomerID;
    END

    CLOSE customer_cursor;
    DEALLOCATE customer_cursor;
END;
GO

--------------------------------------------------------------------------------------------------------------------------------

-- Trigger to auto-generate random and unique OrderNumber on Order insert
CREATE TRIGGER dbo.trg_GenerateOrderNumber
ON dbo.[Order]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OrderID int;
    DECLARE @OrderType nvarchar(8);
    DECLARE @Prefix char(1);
    
    DECLARE order_ordernumber_cursor CURSOR FOR
        SELECT i.OrderID, i.OrderType
        FROM inserted i
        INNER JOIN dbo.[Order] o ON i.OrderID = o.OrderID
        WHERE o.OrderNumber = 'Order00000' OR o.OrderNumber IS NULL;

    OPEN order_ordernumber_cursor;
    FETCH NEXT FROM order_ordernumber_cursor INTO @OrderID, @OrderType;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @Prefix = CASE 
                        WHEN @OrderType = 'Purchase' THEN 'P' 
                        WHEN @OrderType = 'Supply' THEN 'S' 
                        ELSE 'O' -- This should never happen! It is just a fallback
                    END;
                    
        EXEC dbo.AssignRandomNumber @Prefix = @Prefix, @Id = @OrderID;
        FETCH NEXT FROM order_ordernumber_cursor INTO @OrderID, @OrderType;
    END

    CLOSE order_ordernumber_cursor;
    DEALLOCATE order_ordernumber_cursor;
END;
GO

--------------------------------------------------------------------------------------------------------------------------------

-- Trigger to update IsLocked and LockedAt when OrderStatus changes to 3 (Delivered) or 4 (Cancelled)
CREATE TRIGGER dbo.trg_Update_Order_IsLocked_LockedAt
ON dbo.[Order]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @OrderID int;
    DECLARE @OldStatus int;
    DECLARE @NewStatus int;
    DECLARE @OldIsLocked bit;
    
    DECLARE order_islocked_cursor CURSOR FOR
        SELECT i.OrderID, d.OrderStatus, i.OrderStatus, d.IsLocked
        FROM inserted i
        INNER JOIN deleted d ON i.OrderID = d.OrderID
        WHERE i.OrderStatus <> d.OrderStatus;
    
    OPEN order_islocked_cursor;
    FETCH NEXT FROM order_islocked_cursor INTO @OrderID, @OldStatus, @NewStatus, @OldIsLocked;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC dbo.CheckStatusAndLock 
            @Table = 'Order',
            @ID = @OrderID,
            @IDColumn = 'OrderID',
            @StatusColumn = 'OrderStatus',
            @OldStatus = @OldStatus,
            @NewStatus = @NewStatus,
            @LockStatus1 = 3,  -- Delivered
            @LockStatus2 = 4,  -- Cancelled
            @OldIsLocked = @OldIsLocked;
        
        FETCH NEXT FROM order_islocked_cursor INTO @OrderID, @OldStatus, @NewStatus, @OldIsLocked;
    END
    
    CLOSE order_islocked_cursor;
    DEALLOCATE order_islocked_cursor;
END;
GO

--------------------------------------------------------------------------------------------------------------------------------

-- Trigger to update IsLocked and LockedAt when ShipmentStatus changes to 2 (Delivered) or 3 (Cancelled)
CREATE TRIGGER dbo.trg_Update_Shipment_IsLocked_LockedAt
ON dbo.[Shipment]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ShipmentID int;
    DECLARE @OldStatus int;
    DECLARE @NewStatus int;
    DECLARE @OldIsLocked bit;
    
    DECLARE shipment_islocked_cursor CURSOR FOR
        SELECT i.ShipmentID, d.ShipmentStatus, i.ShipmentStatus, d.IsLocked
        FROM inserted i
        INNER JOIN deleted d ON i.ShipmentID = d.ShipmentID
        WHERE i.ShipmentStatus <> d.ShipmentStatus;
    
    OPEN shipment_islocked_cursor;
    FETCH NEXT FROM shipment_islocked_cursor INTO @ShipmentID, @OldStatus, @NewStatus, @OldIsLocked;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC dbo.CheckStatusAndLock 
            @Table = 'Shipment',
            @ID = @ShipmentID,
            @IDColumn = 'ShipmentID',
            @StatusColumn = 'ShipmentStatus',
            @OldStatus = @OldStatus,
            @NewStatus = @NewStatus,
            @LockStatus1 = 2,  -- Delivered
            @LockStatus2 = 3,  -- Failed
            @OldIsLocked = @OldIsLocked;
        
        FETCH NEXT FROM shipment_islocked_cursor INTO @ShipmentID, @OldStatus, @NewStatus, @OldIsLocked;
    END
    
    CLOSE shipment_islocked_cursor;
    DEALLOCATE shipment_islocked_cursor;
END;
GO

--------------------------------------------------------------------------------------------------------------------------------

-- Trigger to prevent updates to Order table with IsLocked = 1
CREATE TRIGGER dbo.trg_PreventUpdate_Order
ON dbo.[Order]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Only prevent updates if the table is locked and the following columns are changed
    -- OrderStatus, IsApproved, ApprovedBy, IsLocked
    IF EXISTS (
        SELECT 1 FROM inserted i
        INNER JOIN deleted d ON i.OrderID = d.OrderID
        WHERE d.IsLocked = 1 AND (
             (i.OrderStatus <> d.OrderStatus) OR
             (i.IsApproved <> d.IsApproved) OR
             (i.ApprovedBy <> d.ApprovedBy) OR
             (i.ApprovalDate <> d.ApprovalDate) OR
             (i.IsLocked <> d.IsLocked))
    )
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('Order table is locked. It cannot be updated.', 16, 1);
    END
END;
GO

--------------------------------------------------------------------------------------------------------------------------------

-- Trigger to prevent updates to Shipment table with IsLocked = 1
CREATE TRIGGER dbo.trg_PreventUpdate_Shipment
ON dbo.[Shipment]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Only prevent updates if the table is locked and the following columns are changed
    -- ShipmentStatus, IsLocked, ActualDeliveryDate
    IF EXISTS (
        SELECT 1 FROM inserted i
        INNER JOIN deleted d ON i.ShipmentID = d.ShipmentID
        WHERE d.IsLocked = 1 AND (
             (i.CustomsDocRef <> d.CustomsDocRef) OR
             (i.ShipmentStatus <> d.ShipmentStatus) OR
             (i.IsLocked <> d.IsLocked) OR
             (i.ExpectedDeliveryDate <> d.ExpectedDeliveryDate) OR
             (i.ActualDeliveryDate <> d.ActualDeliveryDate))
    )
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('Shipment table is locked. It cannot be updated.', 16, 1);
    END
END;
GO

--------------------------------------------------------------------------------------------------------------------------------

-- Trigger to prevent approving orders for unreliable customers with unpaid billings

-- If the customer is unreliable and has unpaid billings
-- We check the total RemainingBalance for Purchase type billings (customer owes us) and    
-- Supply type billings (we owe customer) and prevent approval if the customer owes us more than we owe them
CREATE TRIGGER dbo.trg_PreventApproval_UnreliableCustomer_UnpaidBillings
ON dbo.[Order]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Only check when order is being approved (IsApproved changes from 0 to 1 or ApprovedBy is being set)
    -- Only check Purchase orders, not Supply orders
    IF EXISTS (
        SELECT 1 FROM inserted i
        INNER JOIN deleted d ON i.OrderID = d.OrderID
        WHERE ((i.IsApproved = 1 AND d.IsApproved = 0) OR 
               (i.ApprovedBy IS NOT NULL AND d.ApprovedBy IS NULL))
          AND i.OrderType = 'Purchase'
    )
    BEGIN
        DECLARE @CustomerID int;
        DECLARE @ReliabilityStatus bit;
        DECLARE @PurchaseRemainingBalance decimal(18, 2);  -- Customer owes us
        DECLARE @SupplyRemainingBalance decimal(18, 2);    -- We owe customer
        
        -- Check each Purchase order being approved
        DECLARE approval_cursor CURSOR FOR
            SELECT DISTINCT i.CustomerID
            FROM inserted i
            INNER JOIN deleted d ON i.OrderID = d.OrderID
            WHERE ((i.IsApproved = 1 AND d.IsApproved = 0) OR 
                   (i.ApprovedBy IS NOT NULL AND d.ApprovedBy IS NULL))
              AND i.OrderType = 'Purchase';
        
        OPEN approval_cursor;
        FETCH NEXT FROM approval_cursor INTO @CustomerID;
        
        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- Get customer's reliability status
            SELECT @ReliabilityStatus = ReliabilityStatus
            FROM dbo.[Customer]
            WHERE CustomerID = @CustomerID;
            
            -- If customer is unreliable, check billing balances
            IF @ReliabilityStatus = 0
            BEGIN
                -- Calculate total RemainingBalance for Purchase type billings (customer owes us)
                SELECT @PurchaseRemainingBalance = ISNULL(SUM(RemainingBalance), 0)
                FROM dbo.[Billing]
                WHERE CustomerID = @CustomerID
                  AND BillingType = 'Purchase';
                
                -- Calculate total RemainingBalance for Supply type billings (we owe customer)
                SELECT @SupplyRemainingBalance = ISNULL(SUM(RemainingBalance), 0)
                FROM dbo.[Billing]
                WHERE CustomerID = @CustomerID
                  AND BillingType = 'Supply';
                
                -- If customer owes us more than we owe them, prevent approval
                IF @PurchaseRemainingBalance > @SupplyRemainingBalance
                BEGIN
                    ROLLBACK TRANSACTION;
                    DECLARE @ErrorMessage nvarchar(500);
                    SET @ErrorMessage = 'Cannot approve order: Customer owes more than we owe them. Customer owes ' + 
                                       CAST(@PurchaseRemainingBalance AS nvarchar(20)) + 
                                       ' TL (Purchase billings) and we owe ' + 
                                       CAST(@SupplyRemainingBalance AS nvarchar(20)) + 
                                       ' TL (Supply billings). Customer must pay their outstanding balance before approving this order.';
                    RAISERROR(@ErrorMessage, 16, 1);
                    RETURN;
                END
            END
            
            FETCH NEXT FROM approval_cursor INTO @CustomerID;
        END
        
        CLOSE approval_cursor;
        DEALLOCATE approval_cursor;
    END
END;
GO

--------------------------------------------------------------------------------------------------------------------------------

-- Trigger to delete FinancialTransaction entries when OrderStatus is updated to 4 (Cancelled)
CREATE TRIGGER dbo.trg_Delete_FT_On_Order_Cancelled
ON dbo.[Order]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Only process when OrderStatus changes to 4 (Cancelled)
    IF EXISTS (
        SELECT 1 FROM inserted i
        INNER JOIN deleted d ON i.OrderID = d.OrderID
        WHERE i.OrderStatus = 4 
          AND d.OrderStatus != 4
    )
    BEGIN
        DECLARE @OrderID int;
        DECLARE @FTransactionID int;
        
        -- Process each cancelled order
        DECLARE order_cancelled_cursor CURSOR FOR
            SELECT DISTINCT i.OrderID
            FROM inserted i
            INNER JOIN deleted d ON i.OrderID = d.OrderID
            WHERE i.OrderStatus = 4 
              AND d.OrderStatus != 4;
        
        OPEN order_cancelled_cursor;
        FETCH NEXT FROM order_cancelled_cursor INTO @OrderID;
        
        WHILE @@FETCH_STATUS = 0
        BEGIN
            DECLARE @BillingID int;
            
            -- Get all FinancialTransaction IDs for this order
            DECLARE ft_delete_cursor CURSOR FOR
                SELECT FTransactionID
                FROM dbo.[FinancialTransaction]
                WHERE OrderID = @OrderID;
            
            OPEN ft_delete_cursor;
            FETCH NEXT FROM ft_delete_cursor INTO @FTransactionID;
            
            WHILE @@FETCH_STATUS = 0
            BEGIN
                -- Get BillingID before deletion for Billing update
                SELECT @BillingID = BillingID
                FROM dbo.[FinancialTransaction]
                WHERE FTransactionID = @FTransactionID;
                
                -- Delete Payment records first (FTransactionID is NOT NULL in Payment)
                DELETE FROM dbo.[Payment]
                WHERE FTransactionID = @FTransactionID;
                
                -- Recalculate FT.TotalPaid based on remaining payments (should be 0 after deletion)
                UPDATE dbo.[FinancialTransaction]
                SET TotalPaid = ISNULL((
                    SELECT SUM(PaymentAmount)
                    FROM dbo.[Payment]
                    WHERE FTransactionID = @FTransactionID
                ), 0)
                WHERE FTransactionID = @FTransactionID;
                
                -- Update Treasury records (set FTransactionID to NULL since it's nullable)
                UPDATE dbo.[Treasury]
                SET FTransactionID = NULL
                WHERE FTransactionID = @FTransactionID;
                
                -- Delete the FinancialTransaction (this will trigger Billing.TotalPaid update)
                DELETE FROM dbo.[FinancialTransaction]
                WHERE FTransactionID = @FTransactionID;
                
                -- Explicitly update Billing.TotalDue and TotalPaid after FT deletion
                IF @BillingID IS NOT NULL
                BEGIN
                    UPDATE dbo.[Billing]
                    SET TotalDue = CASE 
                        WHEN ISNULL((
                            SELECT SUM(ft.TotalAmount)
                            FROM dbo.[FinancialTransaction] ft
                            WHERE ft.BillingID = @BillingID
                        ), 0) = 0 THEN 0.01  -- Minimum value to satisfy TotalDue > 0 constraint
                        ELSE ISNULL((
                            SELECT SUM(ft.TotalAmount)
                            FROM dbo.[FinancialTransaction] ft
                            WHERE ft.BillingID = @BillingID
                        ), 0)
                    END,
                    TotalPaid = ISNULL((
                        SELECT SUM(ft.TotalPaid)
                        FROM dbo.[FinancialTransaction] ft
                        WHERE ft.BillingID = @BillingID
                    ), 0),
                    LastUpdatedAt = sysdatetime()
                    WHERE BillingID = @BillingID;
                    
                    -- Update BillingStatus
                    EXEC dbo.Update_BillingStatus @BillingID = @BillingID;
                END
                
                FETCH NEXT FROM ft_delete_cursor INTO @FTransactionID;
            END
            
            CLOSE ft_delete_cursor;
            DEALLOCATE ft_delete_cursor;
            
            FETCH NEXT FROM order_cancelled_cursor INTO @OrderID;
        END
        
        CLOSE order_cancelled_cursor;
        DEALLOCATE order_cancelled_cursor;
    END
END;
GO

--------------------------------------------------------------------------------------------------------------------------------

-- Trigger to update BillingStatus when TotalDue, TotalPaid, or RemainingBalance changes
CREATE TRIGGER dbo.trg_Update_BillingStatus
ON dbo.[Billing]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Only update when the following columns are changed
    -- TotalDue, TotalPaid, RemainingBalance
    IF EXISTS (
        SELECT 1 FROM inserted i
        INNER JOIN deleted d ON i.BillingID = d.BillingID
        WHERE ((i.TotalDue <> d.TotalDue) OR 
            (i.TotalPaid <> d.TotalPaid) OR 
            (i.RemainingBalance <> d.RemainingBalance))
            AND i.TotalDue > 0 AND i.TotalPaid > 0
    )
    BEGIN
        DECLARE @BillingID int = (SELECT BillingID FROM inserted);
        EXEC dbo.Update_BillingStatus @BillingID = @BillingID;
    END
END;
GO

--------------------------------------------------------------------------------------------------------------------------------

-- Trigger to update Billing.TotalPaid when a FinancialTransaction is inserted
CREATE TRIGGER dbo.trg_Update_Billing_TotalPaid_On_FT_Insert

ON dbo.[FinancialTransaction]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Update Billing.TotalPaid for each affected BillingID
    UPDATE b
    SET b.TotalPaid = (
        SELECT ISNULL(SUM(ft.TotalPaid), 0)
        FROM dbo.[FinancialTransaction] ft
        WHERE ft.BillingID = b.BillingID
    ),
    b.LastUpdatedAt = sysdatetime()
    FROM dbo.[Billing] b
    INNER JOIN inserted i ON b.BillingID = i.BillingID;
    
    -- Update BillingStatus for affected Billings
    DECLARE @BillingID int;
    DECLARE billing_ft_insert_cursor CURSOR FOR
        SELECT DISTINCT BillingID FROM inserted;
    
    OPEN billing_ft_insert_cursor;
    FETCH NEXT FROM billing_ft_insert_cursor INTO @BillingID;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC dbo.Update_BillingStatus @BillingID = @BillingID;
        FETCH NEXT FROM billing_ft_insert_cursor INTO @BillingID;
    END
    
    CLOSE billing_ft_insert_cursor;
    DEALLOCATE billing_ft_insert_cursor;
END;
GO

--------------------------------------------------------------------------------------------------------------------------------

-- Trigger to update Billing.TotalPaid when a FinancialTransaction's TotalPaid is updated
CREATE TRIGGER dbo.trg_Update_Billing_TotalPaid_On_FT_Update
ON dbo.[FinancialTransaction]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Only update when TotalPaid changes
    IF EXISTS (
        SELECT 1 FROM inserted i
        INNER JOIN deleted d ON i.FTransactionID = d.FTransactionID
        WHERE i.TotalPaid <> d.TotalPaid
    )
    BEGIN
        -- Update Billing.TotalPaid for each affected BillingID
        UPDATE b
        SET b.TotalPaid = (
            SELECT ISNULL(SUM(ft.TotalPaid), 0)
            FROM dbo.[FinancialTransaction] ft
            WHERE ft.BillingID = b.BillingID
        ),
        b.LastUpdatedAt = sysdatetime()
        FROM dbo.[Billing] b
        INNER JOIN inserted i ON b.BillingID = i.BillingID;
        
        -- Also update Billing.TotalPaid for BillingIDs from deleted records (in case BillingID changed)
        UPDATE b
        SET b.TotalPaid = (
            SELECT ISNULL(SUM(ft.TotalPaid), 0)
            FROM dbo.[FinancialTransaction] ft
            WHERE ft.BillingID = b.BillingID
        ),
        b.LastUpdatedAt = sysdatetime()
        FROM dbo.[Billing] b
        INNER JOIN deleted d ON b.BillingID = d.BillingID
        WHERE NOT EXISTS (SELECT 1 FROM inserted i WHERE i.BillingID = d.BillingID);
        
        -- Update BillingStatus for affected Billings
        DECLARE @BillingID int;
        DECLARE billing_ft_update_cursor CURSOR FOR
            SELECT DISTINCT BillingID FROM inserted
            UNION
            SELECT DISTINCT BillingID FROM deleted;
        
        OPEN billing_ft_update_cursor;
        FETCH NEXT FROM billing_ft_update_cursor INTO @BillingID;
        
        WHILE @@FETCH_STATUS = 0
        BEGIN
            EXEC dbo.Update_BillingStatus @BillingID = @BillingID;
            FETCH NEXT FROM billing_ft_update_cursor INTO @BillingID;
        END
        
        CLOSE billing_ft_update_cursor;
        DEALLOCATE billing_ft_update_cursor;
    END
END;
GO

--------------------------------------------------------------------------------------------------------------------------------

-- Trigger to update Billing.TotalDue and TotalPaid when a FinancialTransaction is deleted
CREATE TRIGGER dbo.trg_Update_Billing_TotalPaid_On_FT_Delete
ON dbo.[FinancialTransaction]
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Update Billing.TotalDue and TotalPaid for each affected BillingID
    UPDATE b
    SET b.TotalDue = CASE 
        WHEN ISNULL((
            SELECT SUM(ft.TotalAmount)
            FROM dbo.[FinancialTransaction] ft
            WHERE ft.BillingID = b.BillingID
        ), 0) = 0 THEN 0.01  -- Minimum value to satisfy TotalDue > 0 constraint
        ELSE ISNULL((
            SELECT SUM(ft.TotalAmount)
            FROM dbo.[FinancialTransaction] ft
            WHERE ft.BillingID = b.BillingID
        ), 0)
    END,
    b.TotalPaid = (
        SELECT ISNULL(SUM(ft.TotalPaid), 0)
        FROM dbo.[FinancialTransaction] ft
        WHERE ft.BillingID = b.BillingID
    ),
    b.LastUpdatedAt = sysdatetime()
    FROM dbo.[Billing] b
    INNER JOIN deleted d ON b.BillingID = d.BillingID;
    
    -- Update BillingStatus for affected Billings
    DECLARE @BillingID int;
    DECLARE billing_ft_delete_cursor CURSOR FOR
        SELECT DISTINCT BillingID FROM deleted;
    
    OPEN billing_ft_delete_cursor;
    FETCH NEXT FROM billing_ft_delete_cursor INTO @BillingID;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC dbo.Update_BillingStatus @BillingID = @BillingID;
        FETCH NEXT FROM billing_ft_delete_cursor INTO @BillingID;
    END
    
    CLOSE billing_ft_delete_cursor;
    DEALLOCATE billing_ft_delete_cursor;
END;
GO

------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

-- Trigger to create treasury entries for each financial transaction
CREATE TRIGGER dbo.trg_FinancialTransaction_CreateTreasuryEntry
ON dbo.[FinancialTransaction]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    -- Insert treasury entries for each transaction
    
    DECLARE @FTransactionID int;
    DECLARE @TransactionType nvarchar(8);
    DECLARE @CurrentBalance decimal(18, 2);
    DECLARE @Amount decimal(18, 2);
    DECLARE @BalanceAfter decimal(18, 2);
    
    -- Get the starting balance (most recent BalanceAfter before inserting new entries)
    SELECT TOP 1 @CurrentBalance = BalanceAfter
    FROM dbo.[Treasury]
    ORDER BY TreasuryID DESC;
    
    -- If no previous balance exists, start with 0
    SET @CurrentBalance = ISNULL(@CurrentBalance, 0);
    
    -- Process each inserted FinancialTransaction
    DECLARE @TotalPaid decimal(18, 2);
    DECLARE ft_treasury_cursor CURSOR FOR
        SELECT FTransactionID, TotalPaid, TransactionType
        FROM inserted
        ORDER BY FTransactionID;
    
    OPEN ft_treasury_cursor;
    FETCH NEXT FROM ft_treasury_cursor INTO @FTransactionID, @TotalPaid, @TransactionType;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Treasury Amount = FT's TotalPaid
        SET @Amount = @TotalPaid;
        
        -- Calculate the signed amount based on TransactionType
        SET @BalanceAfter = @CurrentBalance + CASE 
            WHEN @TransactionType = 'Purchase' THEN @TotalPaid  -- Money in (+)
            WHEN @TransactionType = 'Supply' THEN -@TotalPaid    -- Money out (-)
        END;
        
        -- Insert the Treasury entry
        INSERT INTO dbo.[Treasury] (FTransactionID, Amount, BalanceAfter, Description)
        VALUES (@FTransactionID, @Amount, @BalanceAfter, 'Auto-generated from ' + @TransactionType + ' transaction');
        
        -- Update current balance for next iteration
        SET @CurrentBalance = @BalanceAfter;
        
        FETCH NEXT FROM ft_treasury_cursor INTO @FTransactionID, @TotalPaid, @TransactionType;
    END
    
    CLOSE ft_treasury_cursor;
    DEALLOCATE ft_treasury_cursor;
END;
GO

---------------------------------------------------------------------------------------------------------------------------------

-- Trigger to update Treasury entry when FT.TotalPaid is updated
CREATE TRIGGER dbo.trg_Update_Treasury_On_FT_TotalPaid_Update
ON dbo.[FinancialTransaction]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Only process when TotalPaid changes
    IF EXISTS (
        SELECT 1 FROM inserted i
        INNER JOIN deleted d ON i.FTransactionID = d.FTransactionID
        WHERE i.TotalPaid <> d.TotalPaid
    )
    BEGIN
        DECLARE @FTransactionID int;
        DECLARE @OldTotalPaid decimal(18, 2);
        DECLARE @NewTotalPaid decimal(18, 2);
        DECLARE @TransactionType nvarchar(8);
        DECLARE @OldAmount decimal(18, 2);
        DECLARE @AmountDifference decimal(18, 2);
        DECLARE @NewBalanceAfter decimal(18, 2);
        
        -- Process each updated FinancialTransaction
        DECLARE ft_treasury_update_cursor CURSOR FOR
            SELECT i.FTransactionID, d.TotalPaid, i.TotalPaid, i.TransactionType
            FROM inserted i
            INNER JOIN deleted d ON i.FTransactionID = d.FTransactionID
            WHERE i.TotalPaid <> d.TotalPaid;
        
        OPEN ft_treasury_update_cursor;
        FETCH NEXT FROM ft_treasury_update_cursor INTO @FTransactionID, @OldTotalPaid, @NewTotalPaid, @TransactionType;
        
        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- Get the current Treasury entry for this FT
            SELECT @OldAmount = Amount
            FROM dbo.[Treasury]
            WHERE FTransactionID = @FTransactionID;
            
            -- If Treasury entry exists, update it
            IF @OldAmount IS NOT NULL
            BEGIN
                DECLARE @TreasuryID int;
                
                -- Get the TreasuryID for this FT
                SELECT @TreasuryID = TreasuryID
                FROM dbo.[Treasury]
                WHERE FTransactionID = @FTransactionID;
                
                -- Calculate the difference in amount
                SET @AmountDifference = @NewTotalPaid - @OldTotalPaid;
                
                -- Calculate the balance adjustment based on TransactionType
                DECLARE @BalanceAdjustment decimal(18, 2) = CASE 
                    WHEN @TransactionType = 'Purchase' THEN @AmountDifference  -- Money in (+)
                    WHEN @TransactionType = 'Supply' THEN -@AmountDifference   -- Money out (-)
                END;
                
                -- Get the previous balance (from the entry before this one)
                DECLARE @PreviousBalance decimal(18, 2);
                SELECT TOP 1 @PreviousBalance = BalanceAfter
                FROM dbo.[Treasury]
                WHERE TreasuryID < @TreasuryID
                ORDER BY TreasuryID DESC;
                
                SET @PreviousBalance = ISNULL(@PreviousBalance, 0);
                
                -- Calculate new balance after
                SET @NewBalanceAfter = @PreviousBalance + CASE 
                    WHEN @TransactionType = 'Purchase' THEN @NewTotalPaid  -- Money in (+)
                    WHEN @TransactionType = 'Supply' THEN -@NewTotalPaid   -- Money out (-)
                END;
                
                -- Update Treasury entry
                UPDATE dbo.[Treasury]
                SET Amount = @NewTotalPaid,
                    BalanceAfter = @NewBalanceAfter,
                    Description = 'Auto-generated from ' + @TransactionType + ' transaction',
                    LastUpdatedAt = sysdatetime()
                WHERE FTransactionID = @FTransactionID;
                
                -- Update all subsequent Treasury entries' BalanceAfter
                UPDATE t
                SET BalanceAfter = t.BalanceAfter + @BalanceAdjustment,
                    Description = 'Auto-generated from ' + @TransactionType + ' transaction',
                    LastUpdatedAt = sysdatetime()
                FROM dbo.[Treasury] t
                WHERE t.TreasuryID > @TreasuryID;
            END
            
            FETCH NEXT FROM ft_treasury_update_cursor INTO @FTransactionID, @OldTotalPaid, @NewTotalPaid, @TransactionType;
        END
        
        CLOSE ft_treasury_update_cursor;
        DEALLOCATE ft_treasury_update_cursor;
    END
END;
GO

--------------------------------------------------------------------------------------------------------------------------------

-- Trigger to update FT.TotalPaid when a Payment with that FT's FK is inserted
CREATE TRIGGER dbo.trg_Update_FT_TotalPaid_On_Payment_Insert
ON dbo.[Payment]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Update FT.TotalPaid for each affected FTransactionID
    UPDATE ft
    SET ft.TotalPaid = (
        SELECT ISNULL(SUM(p.PaymentAmount), 0)
        FROM dbo.[Payment] p
        WHERE p.FTransactionID = ft.FTransactionID
    ),
    ft.LastUpdatedAt = sysdatetime()
    FROM dbo.[FinancialTransaction] ft
    INNER JOIN inserted i ON ft.FTransactionID = i.FTransactionID;
    
    -- Update PaymentStatus for affected FinancialTransactions
    DECLARE @FTransactionID int;
    DECLARE ft_payment_insert_cursor CURSOR FOR
        SELECT DISTINCT FTransactionID FROM inserted;
    
    OPEN ft_payment_insert_cursor;
    FETCH NEXT FROM ft_payment_insert_cursor INTO @FTransactionID;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC dbo.Update_FTPaymentStatus @FTransactionID = @FTransactionID;
        FETCH NEXT FROM ft_payment_insert_cursor INTO @FTransactionID;
    END
    
    CLOSE ft_payment_insert_cursor;
    DEALLOCATE ft_payment_insert_cursor;
END;
GO

---------------------------------------------------------------------------------------------------------------------------------

-- Trigger to update FT.TotalPaid when a Payment is deleted
CREATE TRIGGER dbo.trg_Update_FT_TotalPaid_On_Payment_Delete
ON dbo.[Payment]
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Update FT.TotalPaid for each affected FTransactionID
    UPDATE ft
    SET ft.TotalPaid = (
        SELECT ISNULL(SUM(p.PaymentAmount), 0)
        FROM dbo.[Payment] p
        WHERE p.FTransactionID = ft.FTransactionID
    ),
    ft.LastUpdatedAt = sysdatetime()
    FROM dbo.[FinancialTransaction] ft
    INNER JOIN deleted d ON ft.FTransactionID = d.FTransactionID;
    
    -- Update PaymentStatus for affected FinancialTransactions
    DECLARE @FTransactionID int;
    DECLARE ft_payment_delete_cursor CURSOR FOR
        SELECT DISTINCT FTransactionID FROM deleted;
    
    OPEN ft_payment_delete_cursor;
    FETCH NEXT FROM ft_payment_delete_cursor INTO @FTransactionID;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC dbo.Update_FTPaymentStatus @FTransactionID = @FTransactionID;
        FETCH NEXT FROM ft_payment_delete_cursor INTO @FTransactionID;
    END
    
    CLOSE ft_payment_delete_cursor;
    DEALLOCATE ft_payment_delete_cursor;
END;
GO

---------------------------------------------------------------------------------------------------------------------------------

-- Trigger to calculate the order price by doing sum (batchprices)
CREATE TRIGGER dbo.trg_UpdateOrderTotal
ON dbo.[Batch]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Collect affected orders
    DECLARE @AffectedOrders TABLE (OrderID int PRIMARY KEY);

    INSERT INTO @AffectedOrders (OrderID)
    SELECT DISTINCT OrderID FROM inserted
    WHERE OrderID IS NOT NULL

    UNION

    SELECT DISTINCT OrderID FROM deleted
    WHERE OrderID IS NOT NULL;

    -- Recalculate totals
    UPDATE o
    SET TotalAmount =
        ISNULL((
            SELECT SUM(b.BatchPrice)
            FROM dbo.[Batch] b
            WHERE b.OrderID = o.OrderID
        ), 0),
        LastUpdatedAt = sysdatetime()
    FROM dbo.[Order] o
    JOIN @AffectedOrders ao
        ON o.OrderID = ao.OrderID;
END;
GO

---------------------------------------------------------------------------------------------------------------------------------

-- Trigger to update OrderStatus to Shipped (2) when any Shipment of the Order is updated to In Transit (1)
CREATE TRIGGER dbo.trg_UpdateOrderStatusToShipped
ON dbo.[Shipment]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Find all of the Shipments that belong to the same Order and are In Transit (1)
    DECLARE @OrderShipments TABLE (OrderID int, ShipmentID int);
    INSERT INTO @OrderShipments (OrderID, ShipmentID)
    SELECT DISTINCT OrderID, ShipmentID FROM inserted
    WHERE ShipmentStatus = 1;

    -- Check if any of the Shipments for the same Order are In Transit (1)
    IF EXISTS (
        SELECT 1 FROM @OrderShipments os
    )
    BEGIN
        UPDATE dbo.[Order]
        SET OrderStatus = 2,
            LastUpdatedAt = sysdatetime()
        WHERE OrderID IN (SELECT DISTINCT OrderID FROM @OrderShipments);
    END

END;
GO

--------------------------------------------------------------------------------------------------------------------------------
---------------------------------------------------------- INSERT DATA ---------------------------------------------------------
--------------------------------------------------------------------------------------------------------------------------------

-- User Entries --
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Serkan EROL', 'serkan@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Ebrar ÇELİKKAYA', 'ebrar@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Ali KOLDAŞ', 'ali@test.com', '1234', 'Employee');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Rahmet YILMAZ', 'rahmet@test.com', '1234', 'Employee');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Mehmet KARAKAYA', 'mehmet@test.com', '1234', 'Employee');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Ayşe KARAKAYA', 'ayse@test.com', '1234', 'Employee');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Fatma ASLAN', 'fatma@test.com', '1234', 'Employee');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Zeynep HAR', 'zeynep@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Selin GÜNDÜZ', 'selin@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Sümeyye YILMAZ', 'sumeyye@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Nur ÜLKÜ', 'nur@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Mete ÜLKÜ', 'mete@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Miray YAZICI', 'miray@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Yasin ŞENSOY', 'yasin@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Yunus Emir ÖZGÜL', 'yunus@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Talat BULUT', 'talat@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Ahmet BENK', 'ahmet@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Ece KARAKAYA', 'ece@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Elif ÇINAR', 'elif@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Esin KARAKAYA', 'esin@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Emrah KARAKAYA', 'emrah@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Emre KARAKAYA', 'emre@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Burcu KARASLAN', 'burcu@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Deniz KARASLAN', 'deniz@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Erdal ÇALIŞKAN', 'erdal@test.com', '1234', 'Customer');

-- Employee Entries --
INSERT INTO dbo.[Employee] (EmployeeID, EmployeeRole, AccessLevel)
VALUES (3, 'sales', 3);
INSERT INTO dbo.[Employee] (EmployeeID, EmployeeRole, AccessLevel)
VALUES (4, 'local_admin', 4);
INSERT INTO dbo.[Employee] (EmployeeID, EmployeeRole, AccessLevel)
VALUES (5, 'admin', 7);
INSERT INTO dbo.[Employee] (EmployeeID, EmployeeRole, AccessLevel)
VALUES (6, 'sys_admin', 9);
INSERT INTO dbo.[Employee] (EmployeeID, EmployeeRole, AccessLevel)
VALUES (7, 'ceo', 10);

-- Customer Entries --
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (1, 'Person', 1, 'Yozgat', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (2, 'Company', 1, 'istanbul', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (8, 'Company', 1, 'istanbul', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (9, 'Company', 1, 'Bursa', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (10, 'Company', 1, 'Adana', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (11, 'Company', 1, 'Samsun', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (12, 'Company', 1, 'Samsun', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (13, 'Company', 1, 'İzmir', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (14, 'Company', 1, 'Antalya', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (15, 'Company', 1, 'Amsterdam', 'Netherlands');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (16, 'Company', 1, 'İstanbul', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (17, 'Company', 1, 'Tiflis', 'Georgia');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (18, 'Company', 1, 'Berlin', 'Germany');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (19, 'Company', 1, 'Madrid', 'Spain');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (20, 'Company', 1, 'Paris', 'France');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (21, 'Company', 1, 'Rome', 'Italy');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (22, 'Company', 1, 'Istanbul', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (23, 'Company', 1, 'Ankara', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (24, 'Company', 1, 'Izmir', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (25, 'Company', 1, 'Adana', 'Türkiye');

-- Saved Payment Method Entries --
INSERT INTO dbo.[SavedPaymentMethod] (CustomerID, CardNumber, CardType, CardExpirationDate, RecordExpirationDate)
VALUES (1, '1111-5678-9012-3459', 'Debit', '2028-01-01', '2027-01-01');
INSERT INTO dbo.[SavedPaymentMethod] (CustomerID, CardNumber, CardType, CardExpirationDate, RecordExpirationDate)
VALUES (2, '2222-5678-9012-3458', 'Credit', '2027-06-04', '2027-01-01');
INSERT INTO dbo.[SavedPaymentMethod] (CustomerID, CardNumber, CardType, CardExpirationDate, RecordExpirationDate)
VALUES (8, '8888-5678-9012-3457', 'Debit', '2031-02-07', '2027-01-01');
INSERT INTO dbo.[SavedPaymentMethod] (CustomerID, CardNumber, CardType, CardExpirationDate, RecordExpirationDate)
VALUES (9, '9999-5678-9012-3456', 'Credit', '2030-08-10', '2027-01-01');
INSERT INTO dbo.[SavedPaymentMethod] (CustomerID, CardNumber, CardType, CardExpirationDate, RecordExpirationDate)
VALUES (10, '1010-5678-9012-3455', 'Debit', '2029-03-13', '2027-01-01');
INSERT INTO dbo.[SavedPaymentMethod] (CustomerID, CardNumber, CardType, CardExpirationDate, RecordExpirationDate)
VALUES (11, '1111-5678-9012-3454', 'Credit', '2028-09-16', '2027-01-01');
INSERT INTO dbo.[SavedPaymentMethod] (CustomerID, CardNumber, CardType, CardExpirationDate, RecordExpirationDate)
VALUES (12, '1212-5678-9012-3453', 'Debit', '2027-12-19', '2027-01-01');
INSERT INTO dbo.[SavedPaymentMethod] (CustomerID, CardNumber, CardType, CardExpirationDate, RecordExpirationDate)
VALUES (13, '1313-5678-9012-3452', 'Credit', '2027-04-22', '2027-01-01');
INSERT INTO dbo.[SavedPaymentMethod] (CustomerID, CardNumber, CardType, CardExpirationDate, RecordExpirationDate)
VALUES (14, '1414-5678-9012-3451', 'Debit', '2026-07-25', '2027-01-01');
INSERT INTO dbo.[SavedPaymentMethod] (CustomerID, CardNumber, CardType, CardExpirationDate, RecordExpirationDate)
VALUES (15, '1515-5678-9012-3450', 'Credit', '2029-09-26', '2027-01-01');
INSERT INTO dbo.[SavedPaymentMethod] (CustomerID, CardNumber, CardType, CardExpirationDate, RecordExpirationDate)
VALUES (16, '1616-5678-9012-3449', 'Debit', '2030-12-26', '2027-01-01');
INSERT INTO dbo.[SavedPaymentMethod] (CustomerID, CardNumber, CardType, CardExpirationDate, RecordExpirationDate)
VALUES (17, '1717-5678-9012-3448', 'Credit', '2032-12-26', '2027-01-01');

-- Saved Bank Information Entries --
INSERT INTO dbo.[SavedBankInformation] (CustomerID, BankName, AccountNo, IBAN)
VALUES (1, 'Akbank', '1234567890', 'TR01 0006 2000 0000 0006 6700 0001');
INSERT INTO dbo.[SavedBankInformation] (CustomerID, BankName, AccountNo, IBAN)
VALUES (2, 'Garanti Bank', '1234567890', 'TR02 0006 2000 0000 0006 6700 0002');
INSERT INTO dbo.[SavedBankInformation] (CustomerID, BankName, AccountNo, IBAN)
VALUES (8, 'Türkiye İş Bankası', '1234567890', 'TR08 0006 2000 0000 0006 6700 0008');
INSERT INTO dbo.[SavedBankInformation] (CustomerID, BankName, AccountNo, IBAN)
VALUES (9, 'Türkiye İş Bankası', '1234567890', 'TR09 0006 2000 0000 0006 6700 0009');
INSERT INTO dbo.[SavedBankInformation] (CustomerID, BankName, AccountNo, IBAN)
VALUES (18, 'Türkiye İş Bankası', '1234567890', 'TR18 0006 2000 0000 0006 6700 0018');
INSERT INTO dbo.[SavedBankInformation] (CustomerID, BankName, AccountNo, IBAN)
VALUES (19, 'Ziraat Bankası', '1234567890', 'TR19 0006 2000 0000 0006 6700 0019');
INSERT INTO dbo.[SavedBankInformation] (CustomerID, BankName, AccountNo, IBAN)
VALUES (20, 'Ziraat Bankası', '1234567890', 'TR20 0006 2000 0000 0006 6700 0020');
INSERT INTO dbo.[SavedBankInformation] (CustomerID, BankName, AccountNo, IBAN)
VALUES (21, 'Halkbank', '1234567890', 'TR21 0006 2000 0000 0006 6700 0021');
INSERT INTO dbo.[SavedBankInformation] (CustomerID, BankName, AccountNo, IBAN)
VALUES (22, 'Halkbank', '1234567890', 'TR22 0006 2000 0000 0006 6700 0022');
INSERT INTO dbo.[SavedBankInformation] (CustomerID, BankName, AccountNo, IBAN)
VALUES (23, 'Yapı Kredi Bankası', '1234567890', 'TR23 0006 2000 0000 0006 6700 0023');
INSERT INTO dbo.[SavedBankInformation] (CustomerID, BankName, AccountNo, IBAN)
VALUES (24, 'Yapı Kredi Bankası', '1234567890', 'TR24 0006 2000 0000 0006 6700 0024');
INSERT INTO dbo.[SavedBankInformation] (CustomerID, BankName, AccountNo, IBAN)
VALUES (25, 'Yapı Kredi Bankası', '1234567890', 'TR25 0006 2000 0000 0006 6700 0025');

-- Fabric Entries --
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'White', 1.00, 100, 10.00, 'White cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'Red', 1.00, 100, 10.00, 'Red cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'Blue', 1.00, 100, 10.00, 'Blue cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'Green', 1.00, 100, 10.00, 'Green cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'Yellow', 1.00, 100, 10.00, 'Yellow cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'Purple', 1.00, 100, 10.00, 'Purple cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'Orange', 1.00, 100, 10.00, 'Orange cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'Pink', 1.00, 100, 10.00, 'Pink cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'Brown', 1.00, 100, 10.00, 'Brown cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'Black', 1.00, 100, 10.00, 'Black cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'Gray', 1.00, 100, 10.00, 'Gray cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'Silver', 1.00, 100, 10.00, 'Silver cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'Gold', 1.00, 100, 10.00, 'Gold cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'Gold', 1.00, 100, 10.00, 'White polyester fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'White', 1.00, 100, 10.00, 'Red polyester fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'White', 1.00, 100, 10.00, 'Blue polyester fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'White', 1.00, 100, 10.00, 'Green polyester fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'White', 1.00, 100, 10.00, 'Yellow polyester fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'White', 1.00, 100, 10.00, 'Purple polyester fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'White', 1.00, 100, 10.00, 'Orange polyester fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'White', 1.00, 100, 10.00, 'Pink polyester fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'White', 1.00, 100, 10.00, 'Brown polyester fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'White', 1.00, 100, 10.00, 'Black polyester fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'White', 1.00, 100, 10.00, 'Gray polyester fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'White', 1.00, 100, 10.00, 'Silver polyester fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'White', 1.00, 100, 10.00, 'Gold polyester fabric');