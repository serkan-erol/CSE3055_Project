namespace Kismet.Repository.Helpers;

/// <summary>
/// Centralized SQL query location
/// </summary>
public static class SqlQueries
{
    /// <summary>
    /// User table related queries
    /// </summary>
    public static class User
    {
        // Query for UserResponseDto
        public const string GetUserBase = @"
            SELECT 
                u.UserID,
                u.UserType,
                u.UserName,
                u.ContactEmail,
                u.ContactPhone,
                u.CreatedAt,
                u.LastUpdatedAt
            FROM dbo.[User] u";

        public const string InsertUser = @"
            INSERT INTO dbo.[User] (UserName, ContactEmail, ContactPhone, PasswordHash, UserType)
            VALUES (@UserName, @ContactEmail, @ContactPhone, @PasswordHash, @UserType);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        public const string GetUserByEmail = @"
            SELECT 
                u.UserID,
                u.UserType,
                u.ContactEmail,
                u.PasswordHash
            FROM dbo.[User] u WHERE u.ContactEmail = @ContactEmail";

        public const string UpdateUserName = @"
            UPDATE dbo.[User]
            SET 
                UserName = COALESCE(@UserName, UserName),
                LastUpdatedAt = sysdatetime()
            WHERE UserID = @UserID";

        public const string UpdateUserEmail = @"
            UPDATE dbo.[User]
            SET 
                ContactEmail = COALESCE(@ContactEmail, ContactEmail),
                LastUpdatedAt = sysdatetime()
            WHERE UserID = @UserID";

        public const string UpdateUserPhone = @"
            UPDATE dbo.[User]
            SET 
                ContactPhone = COALESCE(@ContactPhone, ContactPhone),
                LastUpdatedAt = sysdatetime()
            WHERE UserID = @UserID";

        public const string UpdateUserPassword = @"
            UPDATE dbo.[User]
            SET 
                PasswordHash = COALESCE(@PasswordHash, PasswordHash),
                LastUpdatedAt = sysdatetime()
            WHERE UserID = @UserID";

        public const string DeleteUser = "DELETE FROM dbo.[User] WHERE UserID = @UserID";
    }

    /// <summary>
    /// Session table related queries
    /// </summary>
    public static class Session
    {
        // Query for getting session info
        public const string GetSessionInfoBase = @"
            SELECT 
                s.SessionID,
                u.UserID,
                s.AccessToken,
                s.RefreshToken,
                s.ATExpiresAt,
                s.RTExpiresAt,
                s.CreatedAt,
                s.LastUpdatedAt
            FROM dbo.[Session] s
            INNER JOIN dbo.[User] u ON u.UserID = s.UserID";
        
        // Query for getting token info
        public const string GetTokenResponseBaseDto = @"
            SELECT 
                s.AccessToken,
                s.RefreshToken,
                s.ATExpiresAt,
                s.RTExpiresAt
            FROM dbo.[Session] s";
        
        // Query for creating a new session
        public const string InsertSession = @"
            INSERT INTO dbo.[Session] (UserID, AccessToken, RefreshToken, ATExpiresAt, RTExpiresAt)
            VALUES (@UserID, @AccessToken, @RefreshToken, @ATExpiresAt, @RTExpiresAt);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        public const string UpdateAccessToken = @"
            UPDATE dbo.[Session]
            SET 
                AccessToken = COALESCE(@AccessToken, AccessToken),
                ATExpiresAt = COALESCE(@ATExpiresAt, ATExpiresAt),
                LastUpdatedAt = sysdatetime()
            WHERE SessionID = @SessionID";
            
        public const string UpdateRefreshToken = @"
            UPDATE dbo.[Session]
            SET 
                RefreshToken = COALESCE(@RefreshToken, RefreshToken),
                RTExpiresAt = COALESCE(@RTExpiresAt, RTExpiresAt),
                LastUpdatedAt = sysdatetime()
            WHERE SessionID = @SessionID";
        
        // Expire the AccessToken by setting the ATExpiresAt to the current time
        // This can be used to logout a user by expiring the AccessToken
        public const string ExpireAccessToken = @"
            UPDATE dbo.[Session]
            SET 
                ATExpiresAt = sysdatetime(),
                LastUpdatedAt = sysdatetime()
            WHERE SessionID = @SessionID";

        // Check if a session is valid
        public const string IsSessionValidBase = @"
            SELECT 
                CASE WHEN s.ATExpiresAt > sysdatetime() AND s.RTExpiresAt > sysdatetime() THEN 1 ELSE 0 END
            FROM dbo.[Session] s
            INNER JOIN dbo.[User] u ON u.UserID = s.UserID";
    }

    /// <summary>
    /// Customer table related queries
    /// </summary>
    public static class Customer
    {
        // Query for CustomerResponseDto (includes User fields)
        public const string GetCustomerBase = @"
            SELECT 
                c.CustomerID,
                c.CustomerNumber,
                c.CustomerType,
                c.ReliabilityStatus,
                c.City,
                c.Country,
                u.UserName,
                u.ContactEmail,
                u.ContactPhone,
                u.CreatedAt,
                u.LastUpdatedAt
            FROM dbo.[Customer] c
            INNER JOIN dbo.[User] u ON u.UserID = c.CustomerID AND u.UserType = c.UserType";

        public const string InsertCustomer = @"
            INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
            VALUES (@CustomerID, @CustomerType, @ReliabilityStatus, @City, @Country);";

        public const string UpdateCustomerType = @"
            UPDATE dbo.[Customer]
            SET 
                CustomerType = COALESCE(@CustomerType, CustomerType)
            WHERE CustomerID = @CustomerID
            EXEC dbo.Update_LastUpdatedAt @Table = 'User', @ID = @CustomerID, @IDColumn = 'UserID';";

        public const string UpdateCustomerReliability = @"
            UPDATE dbo.[Customer]
            SET 
                ReliabilityStatus = COALESCE(@ReliabilityStatus, ReliabilityStatus)
            WHERE CustomerID = @CustomerID
            EXEC dbo.Update_LastUpdatedAt @Table = 'User', @ID = @CustomerID, @IDColumn = 'UserID';";

        public const string UpdateCustomerCity = @"
            UPDATE dbo.[Customer]
            SET 
                City = COALESCE(@City, City)
            WHERE CustomerID = @CustomerID
            EXEC dbo.Update_LastUpdatedAt @Table = 'User', @ID = @CustomerID, @IDColumn = 'UserID';";

        public const string UpdateCustomerCountry = @"
            UPDATE dbo.[Customer]
            SET 
                Country = COALESCE(@Country, Country)
            WHERE CustomerID = @CustomerID
            EXEC dbo.Update_LastUpdatedAt @Table = 'User', @ID = @CustomerID, @IDColumn = 'UserID';";

        public const string DeleteCustomer = "DELETE FROM dbo.[Customer] WHERE CustomerID = @CustomerID";
    }

    /// <summary>
    /// Employee table related queries
    /// </summary>
    public static class Employee
    {
        // Query for EmployeeResponseDto (includes User fields)
        public const string GetEmployeeBase = @"
            SELECT 
                e.EmployeeID,
                e.EmployeeNumber,
                e.EmployeeRole,
                e.AccessLevel,
                u.UserName,
                u.ContactEmail,
                u.ContactPhone,
                u.CreatedAt,
                u.LastUpdatedAt
            FROM dbo.[Employee] e
            INNER JOIN dbo.[User] u ON u.UserID = e.EmployeeID AND u.UserType = e.UserType";

        public const string GetEmployeeRole = @"
            SELECT 
                e.EmployeeRole
            FROM dbo.[Employee] e
            WHERE e.EmployeeID = @EmployeeID";

        public const string GetEmployeeAccessLevel = @"
            SELECT 
                e.AccessLevel
            FROM dbo.[Employee] e
            WHERE e.EmployeeID = @EmployeeID";

        public const string InsertEmployee = @"
            INSERT INTO dbo.[Employee] (EmployeeID, EmployeeRole, AccessLevel)
            VALUES (@EmployeeID, @EmployeeRole, @AccessLevel);";

        public const string UpdateEmployeeRole = @"
            UPDATE dbo.[Employee]
            SET 
                EmployeeRole = COALESCE(@EmployeeRole, EmployeeRole)
            WHERE EmployeeID = @EmployeeID
            EXEC dbo.Update_LastUpdatedAt @Table = 'User', @ID = @EmployeeID, @IDColumn = 'UserID';";

        public const string UpdateEmployeeAccessLevel = @"
            UPDATE dbo.[Employee]
            SET 
                AccessLevel = COALESCE(@AccessLevel, AccessLevel)
            WHERE EmployeeID = @EmployeeID
            EXEC dbo.Update_LastUpdatedAt @Table = 'User', @ID = @EmployeeID, @IDColumn = 'UserID';";

        public const string DeleteEmployee = "DELETE FROM dbo.[Employee] WHERE EmployeeID = @EmployeeID";
    }

    /// <summary>
    /// Order table related queries
    /// </summary>
    public static class Order
    {
        // Base query for orders to be seen by customers (simpler fields, no join)
        public const string GetOrderBaseForCustomer = @"
            SELECT 
                o.OrderID,
                o.CustomerID,
                o.OrderNumber,
                o.OrderType,
                o.TotalAmount,
                o.OrderStatus,
                o.OrderDate,
                o.LastUpdatedAt
            FROM dbo.[Order] o";

        // Base query for orders to be seen by employees (more fields, includes Customer join)
        public const string GetOrderBaseForEmployee = @"
            SELECT 
                o.OrderID,
                c.CustomerID,
                c.ReliabilityStatus,
                o.OrderNumber,
                o.OrderType,
                o.TotalAmount,
                o.OrderStatus,
                o.IsApproved,
                o.ApprovedBy,
                o.ApprovalDate,
                o.IsLocked,
                o.LockedAt,
                o.OrderDate,
                o.LastUpdatedAt
            FROM dbo.[Order] o
            INNER JOIN dbo.[Customer] c ON c.CustomerID = o.CustomerID AND c.UserType = 'Customer'";

        // Query for creating a new order
        public const string InsertOrder = @"
            INSERT INTO dbo.[Order] (CustomerID, TotalAmount, OrderType)
            VALUES (@CustomerID, @TotalAmount, @OrderType);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        // Query for updating the status of an order for a customer
        public const string UpdateOrderStatus = @"
            UPDATE dbo.[Order]
            SET 
                OrderStatus = COALESCE(@OrderStatus, OrderStatus)
            WHERE OrderID = @OrderID
            EXEC dbo.Update_LastUpdatedAt @Table = 'Order', @ID = @OrderID, @IDColumn = 'OrderID';";

        // Query for approving an order
        public const string ApproveOrder = @"
            UPDATE dbo.[Order]
            SET 
                OrderStatus = 1,
                IsApproved = 1,
                ApprovedBy = @ApprovedBy,
                ApprovalDate = sysdatetime()
            WHERE OrderID = @OrderID
            EXEC dbo.Update_LastUpdatedAt @Table = 'Order', @ID = @OrderID, @IDColumn = 'OrderID';";

        // Query for getting the status of an order
        public const string GetOrderStatusById = @"
            SELECT 
                o.OrderStatus
            FROM dbo.[Order] o
            WHERE o.OrderID = @OrderID";

        // Query for checking if an order is locked
        public const string CheckIfOrderIsLocked = @"
            SELECT 
                o.IsLocked
            FROM dbo.[Order] o
            WHERE o.OrderID = @OrderID";
    }

    /// <summary>
    /// Billing table related queries
    /// </summary>
    public static class Billing
    {
        // Query for BillingResponseToEmployeeDto
        public const string GetBillingBaseEmployee = @"
            SELECT *
            FROM dbo.[Billing] b";

        // Query for BillingResponseToCustomerDto
        public const string GetBillingBaseCustomer = @"
            SELECT
                b.BillingID,
                b.CustomerID,
                b.BillingType,
                b.InvoiceNumber,
                b.TotalDue,
                b.TotalPaid,
                b.RemainingBalance,
                b.PaymentTerms,
                b.BillingDate,
                b.LastUpdatedAt
            FROM dbo.[Billing] b";

        // Query for BillingStatusResponseDto
        public const string GetBillingStatusById = @"
            SELECT 
                b.BillingStatus
            FROM dbo.[Billing] b
            WHERE b.BillingID = @BillingID";

        public const string InsertBilling = @"
            INSERT INTO dbo.[Billing] (CustomerID, BillingType, InvoiceNumber, TotalDue, PaymentTerms, BillingDate)
            VALUES (@CustomerID, @BillingType, @InvoiceNumber, @TotalDue, @PaymentTerms, @BillingDate);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        // aaa
        // Depending on our decision on how to implement Billing and FT connections,
        // We may need or we may delete this query.
        public const string UpdateBillingType = @"
            UPDATE dbo.[Billing]
            SET 
                BillingType = COALESCE(@BillingType, BillingType),
                LastUpdatedAt = sysdatetime()
            WHERE BillingID = @BillingID";
            
        public const string UpdateBillingTotalDue = @"
            UPDATE dbo.[Billing]
            SET 
                TotalDue = TotalDue + COALESCE(@TotalDue, 0),
                LastUpdatedAt = sysdatetime()
            WHERE BillingID = @BillingID";
        
        public const string UpdateBillingTotalPaid = @"
            UPDATE dbo.[Billing]
            SET 
                TotalPaid = TotalPaid + COALESCE(@TotalPaid, 0),
                LastUpdatedAt = sysdatetime()
            WHERE BillingID = @BillingID";

        public const string UpdateBillingPaymentTerms = @"
            UPDATE dbo.[Billing]
            SET 
                PaymentTerms = COALESCE(@PaymentTerms, PaymentTerms),
                LastUpdatedAt = sysdatetime()
            WHERE BillingID = @BillingID";

        public const string UpdateBillingDate = @"
            UPDATE dbo.[Billing]
            SET
                BillingDate = COALESCE(@BillingDate, BillingDate),
                LastUpdatedAt = sysdatetime()
            WHERE BillingID = @BillingID";
    }

    /// <summary>
    /// FinancialTransaction table related queries
    /// </summary>
    public static class FinancialTransaction
    {
        // Query for FTResponseToEmployeeDto
        public const string GetFTBaseEmployee = @"
            SELECT *
            FROM dbo.[FinancialTransaction] ft";

        // Query for FTResponseToCustomerDto
        public const string GetFTBaseCustomer = @"
            SELECT 
                ft.FTransactionID,
                ft.CustomerID,
                ft.TransactionType,
                ft.TotalAmount,
                ft.TotalPaid,
                ft.RemainingBalance,
                ft.PaymentStatus,
                ft.Description,
                ft.TransactionDate,
                ft.LastUpdatedAt
            FROM dbo.[FinancialTransaction] ft";

        // Query for FTPaymentStatusResponseDto
        public const string GetFTPaymentStatusById = @"
            SELECT 
                ft.PaymentStatus
            FROM dbo.[FinancialTransaction] ft
            WHERE ft.FTransactionID = @FTransactionID";

        public const string InsertFT = @"
            INSERT INTO dbo.[FinancialTransaction] (CustomerID, BillingID, OrderID, TransactionType, TotalAmount, TotalPaid, Description, TransactionDate)
            VALUES (@CustomerID, @BillingID, @OrderID, @TransactionType, @TotalAmount, @TotalPaid, @Description, @TransactionDate);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        // Query for updating the TotalDue of a FT
        public const string UpdateFTTotalDue = @"
            UPDATE dbo.[FinancialTransaction]
            SET 
                TotalAmount = COALESCE(@TotalAmount, TotalAmount),
                LastUpdatedAt = sysdatetime()
            WHERE FTransactionID = @FTransactionID";

        // Query for updating the TotalPaid of a FT
        public const string UpdateFTTotalPaid = @"
            UPDATE dbo.[FinancialTransaction]
            SET 
                TotalPaid = TotalPaid + COALESCE(@TotalPaid, 0),
                LastUpdatedAt = sysdatetime()
            WHERE FTransactionID = @FTransactionID";
            
        // Query for updating the description of a FT
        public const string UpdateFTDescription = @"
            UPDATE dbo.[FinancialTransaction]
            SET 
                Description = COALESCE(@Description, Description),
                LastUpdatedAt = sysdatetime()
            WHERE FTransactionID = @FTransactionID";

        // Query for finding a suitable Billing entry for a FT
        public const string FindSuitableBillingEntryForFT = @"
            SELECT TOP 1 *
            FROM dbo.[Billing] b
            WHERE b.CustomerID = @CustomerID AND b.BillingType = @TransactionType AND b.BillingDate >= @TransactionDate
            ORDER BY b.BillingDate DESC, b.BillingID DESC";
    }

    /// <summary>
    /// Payment table related queries
    /// </summary>
    public static class Payment
    {
        // Query for PaymentResponseDto
        public const string GetAllPayments = @"
            SELECT *
            FROM dbo.[Payment] p";

        public const string CreatePayment = @"
            INSERT INTO dbo.[Payment] (BillingID, FTransactionID, PaymentAmount, PaymentType, PaymentMethod, ReferenceNumber)
            VALUES (@BillingID, @FTransactionID, @PaymentAmount, @PaymentType, @PaymentMethod, @ReferenceNumber);
            SELECT CAST(SCOPE_IDENTITY() as int);";
    }
  
    public static class Shipment
    {
        // Query for ShipOrder stored procedure
        public const string ShipOrder = "dbo.ShipOrder";

        // Base query for shipments to be seen by customers
        public const string GetShipmentBaseForCustomer = @"
            SELECT 
                s.ShipmentID,
                s.ShipmentStatus,
                s.ShipmentDate,
                s.OriginCountry,
                s.DestinationCountry,
                s.ExpectedDeliveryDate,
                s.ActualDeliveryDate,
                s.CreatedAt
            FROM dbo.[Shipment] s";

        // Base query for shipments to be seen by employees
        public const string GetShipmentBaseForEmployee = @"
            SELECT 
                s.ShipmentID,
                s.OrderID,
                s.CustomsDocRef,
                s.ShipmentStatus,
                s.IsLocked,
                s.LockedAt,
                s.ShipmentDate,
                s.OriginCountry,
                s.DestinationCountry,
                s.ExpectedDeliveryDate,
                s.ActualDeliveryDate,
                s.CreatedAt,
                s.LastUpdatedAt
            FROM dbo.[Shipment] s";

        // Check if shipments already exist for an order
        public const string GetExistingShipmentsByOrderId = @"
            SELECT 
                s.ShipmentID,
                s.ExpectedDeliveryDate,
                COUNT(b.BatchID) as BatchCount
            FROM dbo.[Shipment] s
            LEFT JOIN dbo.[Batch] b ON b.ShipmentID = s.ShipmentID
            WHERE s.OrderID = @OrderID
            GROUP BY s.ShipmentID, s.ExpectedDeliveryDate
            ORDER BY s.ShipmentID";

        // Get all batch IDs for an order
        public const string GetBatchIdsByOrderId = @"
            SELECT BatchID
            FROM dbo.[Batch]
            WHERE OrderID = @OrderID
            ORDER BY BatchID";

        // Insert new shipment
        public const string InsertShipment = @"
            INSERT INTO dbo.[Shipment] (OrderID, ShipmentDate, ExpectedDeliveryDate)
            VALUES (@OrderID, @ShipmentDate, @ExpectedDeliveryDate);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        // Update batch to link it to a shipment
        public const string UpdateBatchShipmentId = @"
            UPDATE dbo.[Batch]
            SET ShipmentID = @ShipmentID
            WHERE BatchID = @BatchID";

        // Update order status to Shipped (2)
        public const string UpdateOrderStatusToShipped = @"
            UPDATE dbo.[Order]
            SET OrderStatus = 2
            WHERE OrderID = @OrderID
            EXEC dbo.Update_LastUpdatedAt @Table = 'Order', @ID = @OrderID, @IDColumn = 'OrderID';";

        // Update expected delivery date
        public const string UpdateExpectedDeliveryDate = @"
            UPDATE dbo.[Shipment]
            SET 
                ExpectedDeliveryDate = @ExpectedDeliveryDate,
                LastUpdatedAt = sysdatetime()
            WHERE ShipmentID = @ShipmentID";

        // Update shipment status
        public const string UpdateShipmentStatus = @"
            UPDATE dbo.[Shipment]
            SET 
                ShipmentStatus = @ShipmentStatus,
                LastUpdatedAt = sysdatetime()
            WHERE ShipmentID = @ShipmentID";

        // Set actual delivery date
        public const string SetActualDeliveryDate = @"
            UPDATE dbo.[Shipment]
            SET 
                ActualDeliveryDate = @ActualDeliveryDate,
                LastUpdatedAt = sysdatetime()
            WHERE ShipmentID = @ShipmentID";

        // Update customs document reference
        public const string UpdateCustomsDocRef = @"
            UPDATE dbo.[Shipment]
            SET 
                CustomsDocRef = @CustomsDocRef,
                LastUpdatedAt = sysdatetime()
            WHERE ShipmentID = @ShipmentID";

        // Lock shipment
        public const string LockShipment = @"
            UPDATE dbo.[Shipment]
            SET 
                IsLocked = 1,
                LockedAt = sysdatetime()
            WHERE ShipmentID = @ShipmentID";

        // Check if shipment is locked
        public const string CheckIfShipmentIsLocked = @"
            SELECT IsLocked
            FROM dbo.[Shipment]
            WHERE ShipmentID = @ShipmentID";

        // Get shipment status by ID
        public const string GetShipmentStatusById = @"
            SELECT ShipmentStatus
            FROM dbo.[Shipment]
            WHERE ShipmentID = @ShipmentID";
    }


    public static class Treasury
    {
        // Get all treasury entries
        public const string GetAllTreasuryEntries = @"
            SELECT 
                TreasuryID,
                FTransactionID,
                Amount,
                Description,
                EntryDate,
                LastUpdatedAt
            FROM dbo.[Treasury]
            ORDER BY TreasuryID DESC";

        // Get treasury entry by ID
        public const string GetTreasuryEntryById = @"
            SELECT 
                TreasuryID,
                FTransactionID,
                Amount,
                Description,
                EntryDate,
                LastUpdatedAt
            FROM dbo.[Treasury]
            WHERE TreasuryID = @TreasuryID";

        // Get current balance
        public const string GetCurrentBalance = @"
            SELECT 
                ISNULL(SUM(Amount), 0) as CurrentBalance,
                MAX(EntryDate) as LastUpdated,
                COUNT(*) as TotalEntries
            FROM dbo.[Treasury]";

        // Get treasury entries by transaction ID
        public const string GetTreasuryEntriesByTransactionId = @"
            SELECT 
                TreasuryID,
                FTransactionID,
                Amount,
                Description,
                EntryDate,
                LastUpdatedAt
            FROM dbo.[Treasury]
            WHERE FTransactionID = @FTransactionID
            ORDER BY TreasuryID";

        // Stored procedures
        // public const string CreateTreasuryEntry = "dbo.CreateTreasuryEntry"; // Now handled by trigger
    }

    public static class CustomerPayment
    {
        // Saved Payment Methods
        public const string GetSavedPaymentMethodsByCustomerId = @"
            SELECT 
                SPMID,
                CustomerID,
                CardNumber,
                CardType,
                CardExpirationDate,
                RecordExpirationDate,
                CreatedAt,
                LastUpdatedAt
            FROM dbo.[SavedPaymentMethod]
            WHERE CustomerID = @CustomerID
            ORDER BY CreatedAt DESC";

        public const string GetSavedPaymentMethodById = @"
            SELECT 
                SPMID,
                CustomerID,
                CardNumber,
                CardType,
                CardExpirationDate,
                RecordExpirationDate,
                CreatedAt,
                LastUpdatedAt
            FROM dbo.[SavedPaymentMethod]
            WHERE SPMID = @SPMID";

        public const string CreateSavedPaymentMethod = @"
            INSERT INTO dbo.[SavedPaymentMethod] 
            (CustomerID, CardNumber, CardType, CardExpirationDate, RecordExpirationDate)
            OUTPUT INSERTED.*
            VALUES (@CustomerID, @CardNumber, @CardType, @CardExpirationDate, @RecordExpirationDate)";

        public const string UpdateSavedPaymentMethod = @"
            UPDATE dbo.[SavedPaymentMethod]
            SET CardNumber = @CardNumber,
                CardType = @CardType,
                CardExpirationDate = @CardExpirationDate,
                RecordExpirationDate = @RecordExpirationDate,
                LastUpdatedAt = GETUTCDATE()
            OUTPUT INSERTED.*
            WHERE SPMID = @SPMID";

        public const string DeleteSavedPaymentMethod = @"
            DELETE FROM dbo.[SavedPaymentMethod]
            WHERE SPMID = @SPMID";

        // Saved Bank Information
        public const string GetSavedBankInformationByCustomerId = @"
            SELECT 
                SBIID,
                CustomerID,
                BankName,
                AccountNo,
                IBAN,
                CreatedAt,
                LastUpdatedAt
            FROM dbo.[SavedBankInformation]
            WHERE CustomerID = @CustomerID
            ORDER BY CreatedAt DESC";

        public const string GetSavedBankInformationById = @"
            SELECT 
                SBIID,
                CustomerID,
                BankName,
                AccountNo,
                IBAN,
                CreatedAt,
                LastUpdatedAt
            FROM dbo.[SavedBankInformation]
            WHERE SBIID = @SBIID";

        public const string CreateSavedBankInformation = @"
            INSERT INTO dbo.[SavedBankInformation] 
            (CustomerID, BankName, AccountNo, IBAN)
            OUTPUT INSERTED.*
            VALUES (@CustomerID, @BankName, @AccountNo, @IBAN)";

        public const string UpdateSavedBankInformation = @"
            UPDATE dbo.[SavedBankInformation]
            SET BankName = @BankName,
                AccountNo = @AccountNo,
                IBAN = @IBAN,
                LastUpdatedAt = GETUTCDATE()
            OUTPUT INSERTED.*
            WHERE SBIID = @SBIID";

        public const string DeleteSavedBankInformation = @"
            DELETE FROM dbo.[SavedBankInformation]
            WHERE SBIID = @SBIID";
    }


    public static class Fabric
    {
        public const string GetAllFabrics = @"
            SELECT 
                FabricID,
                FabricType,
                Composition,
                Color,
                WeightPerUnit,
                StockQuantity,
                Description
            FROM dbo.[Fabric]
            ORDER BY FabricID";

        public const string GetFabricById = @"
            SELECT 
                FabricID,
                FabricType,
                Composition,
                Color,
                WeightPerUnit,
                StockQuantity,
                Description
            FROM dbo.[Fabric]
            WHERE FabricID = @FabricID";

        public const string InsertFabric = @"
            INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice,Description)
            VALUES (@FabricType, @Composition, @Color, @WeightPerUnit, @StockQuantity, @UnitPrice, @Description);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        public const string UpdateFabricStock = @"
            UPDATE dbo.[Fabric]
            SET StockQuantity = StockQuantity + @QuantityChange
            WHERE FabricID = @FabricID";
    }

    public static class Batch
    {
        public const string GetBatchesForCustomer = @"
            SELECT 
                BatchNumber,
                Quantity,
                BatchPrice,
                ProductionDate,
                QualityGrade
            FROM dbo.[Batch]
            WHERE OrderID = @OrderID
            ORDER BY BatchID";

        public const string GetBatchesForEmployee = @"
            SELECT 
                BatchID,
                OrderID,
                ShipmentID,
                FabricID,
                BatchNumber,
                Quantity,
                BatchPrice,
                ProductionDate,
                QualityGrade,
                CreatedAt
            FROM dbo.[Batch]
            WHERE OrderID = @OrderID
            ORDER BY BatchID";

        public const string GetBatchById = @"
            SELECT 
                BatchID,
                OrderID,
                ShipmentID,
                FabricID,
                BatchNumber,
                Quantity,
                BatchPrice,
                ProductionDate,
                QualityGrade,
                CreatedAt
            FROM dbo.[Batch]
            WHERE BatchID = @BatchID";

        public const string GetUnshippedBatches = @"
            SELECT 
                BatchID,
                OrderID,
                ShipmentID,
                FabricID,
                BatchNumber,
                Quantity,
                BatchPrice,
                ProductionDate,
                QualityGrade,
                CreatedAt
            FROM dbo.[Batch]
            WHERE OrderID = @OrderID
            AND ShipmentID IS NULL
            ORDER BY BatchID";

        // Stored procedure to create batches for multiple fabrics
        public const string CreateBatchesForMultipleFabrics = "dbo.CreateBatchesForMultipleFabrics";

        // Query for calculating the total amount of an order
        public const string CalculateOrderTotalAmount = @"
            SELECT SUM(BatchPrice) as TotalAmount
            FROM dbo.[Batch]
            WHERE OrderID = @OrderID";
    }
}