
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
    ContactPhone        nvarchar(20) UNIQUE NULL,
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
    TotalAmount     decimal(18, 2) NOT NULL CHECK (TotalAmount > 0),
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
    ---BalanceAfter    decimal(18, 2) NOT NULL,
    Description     nvarchar(255) NULL,
    EntryDate       datetime2 NOT NULL DEFAULT sysdatetime(),
    LastUpdatedAt   datetime2 NULL,

    CONSTRAINT FK_Treasury_Transaction
        FOREIGN KEY (FTransactionID) REFERENCES dbo.[FinancialTransaction](FTransactionID)
);

-- Payment --
CREATE TABLE dbo.[Payment] (
    PaymentID        int IDENTITY PRIMARY KEY,
    BillingID        int NOT NULL,
    FTransactionID   int NOT NULL,
    PaymentAmount    decimal(18, 2) NOT NULL CHECK (PaymentAmount > 0),
    -- 'Purchase' for the customer paying us (associated with a Purchase type Order / FinancialTransaction) 
    -- and 'Supply' for us paying the customer (associated with a Supply type Order / FinancialTransaction)
    PaymentType      nvarchar(8) NOT NULL CHECK (PaymentType IN ('Purchase', 'Supply')),
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

-- Procedure to update BillingStatus based on TotalDue and TotalPaid
CREATE PROCEDURE dbo.Update_BillingStatus
    @BillingID int
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.[Billing]
    SET BillingStatus = CASE
        WHEN TotalDue = TotalPaid THEN 2
        WHEN TotalDue > TotalPaid THEN 1
        WHEN TotalPaid = 0 THEN 0
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
        WHEN TotalAmount = TotalPaid THEN 2
        WHEN TotalAmount > TotalPaid THEN 1
        WHEN TotalPaid = 0 THEN 0
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

        -- Update order status to Shipped (2)
        UPDATE dbo.[Order]
        SET OrderStatus = 2
        WHERE OrderID = @OrderID;
        
        EXEC dbo.Update_LastUpdatedAt @Table = 'Order', @ID = @OrderID, @IDColumn = 'OrderID';

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
END
GO

--------------------------------------------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.CreateBatches
    @OrderID int,
    @FabricID int,
    @TotalFabricUnits int,
    @QualityGrade nvarchar(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if batches already exist for this order
    IF EXISTS (SELECT 1 FROM dbo.[Batch] WHERE OrderID = @OrderID)
    BEGIN
        RAISERROR('already batched !', 16, 1);
        RETURN;
    END

    IF @TotalFabricUnits < 1
    BEGIN
        RAISERROR('Total fabric units must be at least 1', 16, 1);
        RETURN;
    END

    DECLARE @UnitPrice int;

    SELECT @UnitPrice = UnitPrice
    FROM dbo.[Fabric]
    WHERE FabricID = @FabricID;

    IF @UnitPrice IS NULL
    BEGIN
        RAISERROR('Fabric not found', 16, 1);
        RETURN;
    END

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

        SET @BatchNumber =
            'BATCH-' + FORMAT(GETDATE(), 'yyyyMMddHHmmss') + '-' +
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
END
GO

--------------------------------------------------------------------------------------------------------------------------------
---Create order with batches 
CREATE OR ALTER PROCEDURE dbo.CreateOrderWithBatches
    @CustomerID int,
    @OrderType nvarchar(8),   -- Purchase / Supply
    @FabricID int,
    @TotalFabricUnits int,
    @QualityGrade nvarchar(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @TotalFabricUnits < 1
    BEGIN
        RAISERROR('Quantity must be at least 1', 16, 1);
        RETURN;
    END

    DECLARE @CurrentStock int;

    SELECT @CurrentStock = StockQuantity
    FROM dbo.[Fabric]
    WHERE FabricID = @FabricID;

    IF @CurrentStock IS NULL
    BEGIN
        RAISERROR('Fabric not found', 16, 1);
        RETURN;
    END

    IF @CurrentStock < @TotalFabricUnits
    BEGIN
        RAISERROR('Insufficient fabric stock', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;
    BEGIN TRY
        DECLARE @OrderID int;

        -- 1. Create order
        INSERT INTO dbo.[Order] (
            CustomerID,
            OrderType
        )
        VALUES (
            @CustomerID,
            @OrderType
        );

        SET @OrderID = SCOPE_IDENTITY();

        -- 2. Update stock
        UPDATE dbo.[Fabric]
        SET StockQuantity = StockQuantity - @TotalFabricUnits
        WHERE FabricID = @FabricID;

        -- 3. Create batches
        EXEC dbo.CreateBatches
            @OrderID = @OrderID,
            @FabricID = @FabricID,
            @TotalFabricUnits = @TotalFabricUnits,
            @QualityGrade = @QualityGrade;

        COMMIT TRANSACTION;

        SELECT @OrderID AS OrderID;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
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
    
    DECLARE order_cursor CURSOR FOR
        SELECT i.OrderID, i.OrderType
        FROM inserted i
        INNER JOIN dbo.[Order] o ON i.OrderID = o.OrderID
        WHERE o.OrderNumber = 'Order00000' OR o.OrderNumber IS NULL;

    OPEN order_cursor;
    FETCH NEXT FROM order_cursor INTO @OrderID, @OrderType;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @Prefix = CASE 
                        WHEN @OrderType = 'Purchase' THEN 'P' 
                        WHEN @OrderType = 'Supply' THEN 'S' 
                        ELSE 'O' -- This should never happen! It is just a fallback
                    END;
                    
        EXEC dbo.AssignRandomNumber @Prefix = @Prefix, @Id = @OrderID;
        FETCH NEXT FROM order_cursor INTO @OrderID, @OrderType;
    END

    CLOSE order_cursor;
    DEALLOCATE order_cursor;
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
    
    -- Only update when the following columns are changed
    -- OrderStatus
    IF EXISTS (
        SELECT 1 FROM inserted i
        INNER JOIN deleted d ON i.OrderID = d.OrderID
        WHERE (i.OrderStatus <> d.OrderStatus
            AND i.OrderStatus IN (3, 4)
            AND (d.IsLocked = 0 OR d.LockedAt IS NULL))
    )
    BEGIN
        DECLARE @OrderID int = (SELECT OrderID FROM inserted);
        EXEC dbo.Update_IsLocked_LockedAt @Table = 'Order', @ID = @OrderID, @IDColumn = 'OrderID';
    END
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
    
    -- Only update when the following columns are changed
    -- ShipmentStatus
    IF EXISTS (
        SELECT 1 FROM inserted i
        INNER JOIN deleted d ON i.ShipmentID = d.ShipmentID
        WHERE (i.ShipmentStatus <> d.ShipmentStatus
            AND i.ShipmentStatus IN (2, 3)
            AND (d.IsLocked = 0 OR d.LockedAt IS NULL))
    )
    BEGIN
        DECLARE @ShipmentID int = (SELECT ShipmentID FROM inserted);
        EXEC dbo.Update_IsLocked_LockedAt @Table = 'Shipment', @ID = @ShipmentID, @IDColumn = 'ShipmentID';
    END
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
             (i.ApprovedBy <> d.ApprovedBy))
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
             (i.ShipmentStatus <> d.ShipmentStatus) OR
             (i.IsLocked <> d.IsLocked) OR
             (i.ActualDeliveryDate <> d.ActualDeliveryDate))
    )
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('Shipment table is locked. It cannot be updated.', 16, 1);
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
    DECLARE billing_cursor CURSOR FOR
        SELECT DISTINCT BillingID FROM inserted;
    
    OPEN billing_cursor;
    FETCH NEXT FROM billing_cursor INTO @BillingID;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC dbo.Update_BillingStatus @BillingID = @BillingID;
        FETCH NEXT FROM billing_cursor INTO @BillingID;
    END
    
    CLOSE billing_cursor;
    DEALLOCATE billing_cursor;
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
        DECLARE billing_cursor CURSOR FOR
            SELECT DISTINCT BillingID FROM inserted
            UNION
            SELECT DISTINCT BillingID FROM deleted;
        
        OPEN billing_cursor;
        FETCH NEXT FROM billing_cursor INTO @BillingID;
        
        WHILE @@FETCH_STATUS = 0
        BEGIN
            EXEC dbo.Update_BillingStatus @BillingID = @BillingID;
            FETCH NEXT FROM billing_cursor INTO @BillingID;
        END
        
        CLOSE billing_cursor;
        DEALLOCATE billing_cursor;
    END
END;
GO

--------------------------------------------------------------------------------------------------------------------------------

-- Trigger to update Billing.TotalPaid when a FinancialTransaction is deleted
-- This is redundant. We do NOT plan to delete FinancialTransactions.
CREATE TRIGGER dbo.trg_Update_Billing_TotalPaid_On_FT_Delete
ON dbo.[FinancialTransaction]
AFTER DELETE
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
    INNER JOIN deleted d ON b.BillingID = d.BillingID;
    
    -- Update BillingStatus for affected Billings
    DECLARE @BillingID int;
    DECLARE billing_cursor CURSOR FOR
        SELECT DISTINCT BillingID FROM deleted;
    
    OPEN billing_cursor;
    FETCH NEXT FROM billing_cursor INTO @BillingID;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC dbo.Update_BillingStatus @BillingID = @BillingID;
        FETCH NEXT FROM billing_cursor INTO @BillingID;
    END
    
    CLOSE billing_cursor;
    DEALLOCATE billing_cursor;
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
    DECLARE payment_cursor CURSOR FOR
        SELECT DISTINCT FTransactionID FROM inserted;
    
    OPEN payment_cursor;
    FETCH NEXT FROM payment_cursor INTO @FTransactionID;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC dbo.Update_FTPaymentStatus @FTransactionID = @FTransactionID;
        FETCH NEXT FROM payment_cursor INTO @FTransactionID;
    END
    
    CLOSE payment_cursor;
    DEALLOCATE payment_cursor;
END;

------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

-- Trigger to create treasury entries for each financial transaction
CREATE TRIGGER trg_FinancialTransaction_CreateTreasuryEntry
ON dbo.[FinancialTransaction]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    -- Insert treasury entries for each transaction
    
    INSERT INTO dbo.[Treasury] (FTransactionID, Amount, Description)
    SELECT 
        FTransactionID,
        CASE 
            WHEN TransactionType = 'Purchase' THEN TotalAmount  -- Money in (+)
            WHEN TransactionType = 'Supply' THEN -TotalAmount   -- Money out (-)
        END as Amount,
        'Auto-generated from ' + TransactionType + ' transaction'
    FROM inserted;
END
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
END
GO
