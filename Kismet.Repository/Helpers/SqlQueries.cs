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
        public const string GetAllDto = @"
            SELECT 
                u.UserID,
                u.UserType,
                u.UserName,
                u.ContactEmail,
                u.ContactPhone,
                u.CreatedAt,
                u.LastUpdatedAt
            FROM dbo.[User] u
            ORDER BY u.UserID";

        public const string GetByIdDto = @"
            SELECT 
                u.UserID,
                u.UserType,
                u.UserName,
                u.ContactEmail,
                u.ContactPhone,
                u.CreatedAt,
                u.LastUpdatedAt
            FROM dbo.[User] u
            WHERE u.UserID = @UserID";

        public const string InsertCustomerUser = @"
            INSERT INTO dbo.[User] (UserName, ContactEmail, ContactPhone, PasswordHash, UserType)
            VALUES (@UserName, @ContactEmail, @ContactPhone, @PasswordHash, 'Customer');
            SELECT CAST(SCOPE_IDENTITY() as int);";

        public const string InsertEmployeeUser = @"
            INSERT INTO dbo.[User] (UserName, ContactEmail, ContactPhone, PasswordHash, UserType)
            VALUES (@UserName, @ContactEmail, @ContactPhone, @PasswordHash, 'Employee');
            SELECT CAST(SCOPE_IDENTITY() as int);";

        public const string UpdateUserName = @"
            UPDATE dbo.[User]
            SET 
                UserName = COALESCE(@UserName, UserName)
            WHERE UserID = @UserID";

        public const string UpdateUserEmail = @"
            UPDATE dbo.[User]
            SET 
                ContactEmail = COALESCE(@ContactEmail, ContactEmail)
            WHERE UserID = @UserID";

        public const string UpdateUserPhone = @"
            UPDATE dbo.[User]
            SET 
                ContactPhone = COALESCE(@ContactPhone, ContactPhone)
            WHERE UserID = @UserID";

        public const string UpdateUserPassword = @"
            UPDATE dbo.[User]
            SET 
                PasswordHash = COALESCE(@PasswordHash, PasswordHash)

            WHERE UserID = @UserID";

        public const string DeleteUser = "DELETE FROM dbo.[User] WHERE UserID = @UserID";
    }

    /// <summary>
    /// Customer table related queries
    /// </summary>
    public static class Customer
    {
        // Query for CustomerResponseDto (includes User fields)
        public const string GetAllDto = @"
            SELECT 
                c.CustomerID,
                c.CustomerNumber,
                c.CustomerType,
                c.ReliabilityStatus,
                u.UserName,
                u.ContactEmail,
                u.ContactPhone,
                u.CreatedAt,
                u.LastUpdatedAt
            FROM dbo.[Customer] c
            INNER JOIN dbo.[User] u ON u.UserID = c.CustomerID AND u.UserType = c.UserType
            ORDER BY c.CustomerID";

        public const string GetByIdDto = @"
            SELECT 
                c.CustomerID,
                c.CustomerNumber,
                c.CustomerType,
                c.ReliabilityStatus,
                u.UserName,
                u.ContactEmail,
                u.ContactPhone,
                u.CreatedAt,
                u.LastUpdatedAt
            FROM dbo.[Customer] c
            INNER JOIN dbo.[User] u ON u.UserID = c.CustomerID AND u.UserType = c.UserType
            WHERE c.CustomerID = @CustomerID";

        public const string InsertCustomer = @"
            INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus)
            VALUES (@CustomerID, @CustomerType, @ReliabilityStatus);";

        public const string UpdateCustomerType = @"
            UPDATE dbo.[Customer]
            SET 
                CustomerType = COALESCE(@CustomerType, CustomerType)
            WHERE CustomerID = @CustomerID";

        public const string UpdateCustomerReliability = @"
            UPDATE dbo.[Customer]
            SET 
                ReliabilityStatus = COALESCE(@ReliabilityStatus, ReliabilityStatus)
            WHERE CustomerID = @CustomerID";

        public const string DeleteCustomer = "DELETE FROM dbo.[Customer] WHERE CustomerID = @CustomerID";
    }

    /// <summary>
    /// Employee table related queries
    /// </summary>
    public static class Employee
    {
        // Query for EmployeeResponseDto (includes User fields)
        public const string GetAllDto = @"
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
            INNER JOIN dbo.[User] u ON u.UserID = e.EmployeeID AND u.UserType = e.UserType
            ORDER BY e.EmployeeID";

        public const string GetByIdDto = @"
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
            INNER JOIN dbo.[User] u ON u.UserID = e.EmployeeID AND u.UserType = e.UserType
            WHERE e.EmployeeID = @EmployeeID";

        public const string InsertEmployee = @"
            INSERT INTO dbo.[Employee] (EmployeeID, EmployeeRole, AccessLevel)
            VALUES (@EmployeeID, @EmployeeRole, @AccessLevel);";

        public const string UpdateEmployeeRole = @"
            UPDATE dbo.[Employee]
            SET 
                EmployeeRole = COALESCE(@EmployeeRole, EmployeeRole)
            WHERE EmployeeID = @EmployeeID";

        public const string UpdateEmployeeAccessLevel = @"
            UPDATE dbo.[Employee]
            SET 
                AccessLevel = COALESCE(@AccessLevel, AccessLevel)
            WHERE EmployeeID = @EmployeeID";

        public const string DeleteEmployee = "DELETE FROM dbo.[Employee] WHERE EmployeeID = @EmployeeID";
    }

    /// <summary>
    /// Order table related queries
    /// </summary>
    public static class Order
    {
        // Query for all orders of a customer to be seen by the customer
        public const string GetOrderForCustomer = @"
            SELECT 
                o.OrderNumber,
                o.OrderType,
                o.TotalAmount,
                o.OrderStatus,
                o.OrderDate,
                o.LastUpdatedAt
            FROM dbo.[Order] o
            WHERE o.CustomerID = @CustomerID
            ORDER BY o.OrderID";

        // Query for OrderResponseToEmployeeDto. This brings ALL THE ORDERS!!!
        // And some related Customer data
        // To be seen by an employee
        public const string GetOrderForEmployee = @"
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
            INNER JOIN dbo.[Customer] c ON c.CustomerID = o.CustomerID AND c.UserType = 'Customer'
            ORDER BY o.OrderID";

        // Query for a specific order of a customer to be seen by the customer
        public const string GetOrderByOrderIdForCustomer = @"
            SELECT 
                o.OrderNumber,
                o.OrderType,
                o.TotalAmount,
                o.OrderStatus,
                o.OrderDate,
                o.LastUpdatedAt
            FROM dbo.[Order] o
            WHERE o.OrderID = @OrderID AND o.CustomerID = @CustomerID";

        // Query for a specific order to be seen by an employee
        public const string GetOrderByOrderIdForEmployee = @"
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
            INNER JOIN dbo.[Customer] c ON c.CustomerID = o.CustomerID AND c.UserType = 'Customer'
            WHERE o.OrderID = @OrderID";

        // Query for a specific order to be seen by an employee
        public const string GetOrderByCustomerIdForEmployee = @"
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
            INNER JOIN dbo.[Customer] c ON c.CustomerID = o.CustomerID AND c.UserType = 'Customer'
            WHERE o.CustomerID = @CustomerID";

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
            WHERE OrderID = @OrderID";

        // Query for approving an order
        public const string ApproveOrder = @"
            UPDATE dbo.[Order]
            SET 
                OrderStatus = 1,
                IsApproved = 1,
                ApprovedBy = @ApprovedBy,
                ApprovalDate = sysdatetime()
            WHERE OrderID = @OrderID";

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
}