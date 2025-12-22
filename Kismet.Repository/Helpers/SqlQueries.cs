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
}