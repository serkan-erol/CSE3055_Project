
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