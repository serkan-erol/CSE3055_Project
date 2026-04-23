
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
    DECLARE @NumberColumn nvarchar(50);
    DECLARE @IDColumn     nvarchar(50);

    IF @Prefix NOT IN ('E', 'C', 'P', 'S')
    BEGIN
        RAISERROR('Invalid prefix. Must be ''E'' for Employee, ''C'' for Customer, ''P'' for Purchase Order, or ''S'' for Supply Order.', 16, 1);
        RETURN;
    END

    -- Set table name based on prefix
    SET @TableName = CASE WHEN @Prefix = 'E' THEN 'Employee' 
                          WHEN @Prefix = 'C' THEN 'Customer' 
                          WHEN @Prefix = 'P' OR @Prefix = 'S' THEN 'Order' END;

    -- Set Number column name based on prefix
    SET @NumberColumn = CASE WHEN @Prefix = 'E' THEN 'EmployeeNumber' 
                          WHEN @Prefix = 'C' THEN 'CustomerNumber' 
                          WHEN @Prefix = 'P' OR @Prefix = 'S' THEN 'OrderNumber' END;

    -- Set ID column name based on prefix
    SET @IDColumn = CASE WHEN @Prefix = 'E' THEN 'EmployeeID' 
                          WHEN @Prefix = 'C' THEN 'CustomerID' 
                          WHEN @Prefix = 'P' OR @Prefix = 'S' THEN 'OrderID' END;

    -- Try until we find a unique number or hit the attempt limit
    WHILE @Exists = 1 AND @Attempts < @MaxAttempts
    BEGIN
        SET @Attempts += 1;

        -- 9 random digits
        SET @RandomDigits = RIGHT('000000000'
                                  + CAST(ABS(CHECKSUM(NEWID())) % 1000000000 AS varchar(9)), 9);
        -- 10 digit complete number
        SET @GeneratedNumber = @Prefix + @RandomDigits;

        -- Check if the generated number already exists using dynamic SQL
        DECLARE @CheckSQL nvarchar(MAX);
        DECLARE @ExistsResult int = 0;
        
        -- Create the SQL code to check if the generated number already exists in the corresponding table
        SET @CheckSQL = N'SELECT @ExistsResult = CASE WHEN EXISTS (
            SELECT 1 FROM ' + QUOTENAME('dbo') + N'.' + QUOTENAME(@TableName) + N' 
            WHERE ' + QUOTENAME(@NumberColumn) + N' = @GeneratedNumber
        ) THEN 1 ELSE 0 END';
        
        -- Execute the SQL code
        EXEC sp_executesql @CheckSQL, 
            N'@GeneratedNumber char(10), @ExistsResult int OUTPUT', 
            @GeneratedNumber = @GeneratedNumber, 
            @ExistsResult = @ExistsResult OUTPUT;
        
        -- If the generated number does not exist, set the @Exists flag to 0
        IF @ExistsResult = 0
            SET @Exists = 0;
    END

    -- If the generated number does not exist after all attempts, raise an error
    IF @Exists = 1
    BEGIN
        RAISERROR('Unable to generate unique %sNumber after %d attempts.', 16, 1, @TableName, @MaxAttempts);
        RETURN;
    END

    -- Persist the generated number on the correct table
    DECLARE @SQL nvarchar(MAX);
    -- Create the SQL code to update the number column with the generated number and set the table name, number column and ID column
    SET @SQL = N'UPDATE ' + QUOTENAME('dbo') + N'.' + QUOTENAME(@TableName) + N' 
                 SET ' + QUOTENAME(@NumberColumn) + N' = @GeneratedNumber
                 WHERE ' + QUOTENAME(@IDColumn) + N' = @Id';

    -- Execute the SQL statement
    EXEC sp_executesql @SQL, N'@GeneratedNumber nvarchar(10), @ID int', @GeneratedNumber = @GeneratedNumber, @ID = @Id;
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
    @EmployeeID int,
    @CustomsDocRef nvarchar(100)
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
            INSERT INTO dbo.[Shipment] (OrderID, CustomsDocRef, ShipmentDate, ExpectedDeliveryDate)
            VALUES (@OrderID, @CustomsDocRef, @ShipDate, @ExpectedDelivery);
            
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