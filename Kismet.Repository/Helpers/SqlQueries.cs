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
        // Query for User entity (minimal fields)
        // Currently unused!
        public const string GetAll = @"
            SELECT 
                u.UserID,
                u.UserType,
                u.UserName,
                u.ContactEmail,
                u.ContactPhone
            FROM dbo.[User] u
            ORDER BY u.UserID";
            
        // Currently unused!
        public const string GetById = @"
            SELECT 
                u.UserID,
                u.UserType,
                u.UserName,
                u.ContactEmail,
                u.ContactPhone
            FROM dbo.[User] u
            WHERE u.UserID = @UserID";

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
        // Query for Customer entity (minimal fields)
        // Currently unused!
        public const string GetAll = @"
            SELECT 
                c.CustomerID,
                c.UserType,
                c.CustomerNumber,
                c.CustomerType,
                c.ReliabilityStatus
            FROM dbo.[Customer] c
            INNER JOIN dbo.[User] u ON u.UserID = c.CustomerID AND u.UserType = c.UserType
            ORDER BY c.CustomerID";
            
        // Currently unused!
        public const string GetById = @"
            SELECT 
                c.CustomerID,
                c.UserType,
                c.CustomerNumber,
                c.CustomerType,
                c.ReliabilityStatus
            FROM dbo.[Customer] c
            INNER JOIN dbo.[User] u ON u.UserID = c.CustomerID AND u.UserType = c.UserType
            WHERE c.CustomerID = @CustomerID";

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
                u.CreatedAt
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
                u.CreatedAt
            FROM dbo.[Customer] c
            INNER JOIN dbo.[User] u ON u.UserID = c.CustomerID AND u.UserType = c.UserType
            WHERE c.CustomerID = @CustomerID";

        public const string InsertCustomer = @"
            INSERT INTO dbo.[Customer] (CustomerID, CustomerNumber, CustomerType, ReliabilityStatus)
            VALUES (@CustomerID, @CustomerNumber, @CustomerType, @ReliabilityStatus);";

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
        // Query for Employee entity (minimal fields)
        // Currently unused!
        public const string GetAll = @"
            SELECT 
                e.EmployeeID,
                e.UserType,
                e.EmployeeNumber,
                e.EmployeeRole,
                e.AccessLevel
            FROM dbo.[Employee] e
            INNER JOIN dbo.[User] u ON u.UserID = e.EmployeeID AND u.UserType = e.UserType
            ORDER BY e.EmployeeID";
            
        // Currently unused!
        public const string GetById = @"
            SELECT 
                e.EmployeeID,
                e.UserType,
                e.EmployeeNumber,
                e.EmployeeRole,
                e.AccessLevel
            FROM dbo.[Employee] e
            INNER JOIN dbo.[User] u ON u.UserID = e.EmployeeID AND u.UserType = e.UserType
            WHERE e.EmployeeID = @EmployeeID";

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
                u.CreatedAt
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
                u.CreatedAt
            FROM dbo.[Employee] e
            INNER JOIN dbo.[User] u ON u.UserID = e.EmployeeID AND u.UserType = e.UserType
            WHERE e.EmployeeID = @EmployeeID";

        public const string InsertEmployee = @"
            INSERT INTO dbo.[Employee] (EmployeeID, EmployeeNumber, EmployeeRole, AccessLevel)
            VALUES (@EmployeeID, @EmployeeNumber, @EmployeeRole, @AccessLevel);";

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
}