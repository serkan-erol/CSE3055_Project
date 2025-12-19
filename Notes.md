# CSE3055_Project_TODO_Notes
1 - Keep adding triggers for updating the LastUpdatedAt fields of tables. 
    1.1 - This takes too much space, and creates too much duplicate code. Maybe, we should just send "LastupdatedAt = sysdatetime()" with queries that require it?
2 - Add everything for FinancialTransaction, Payment, Treasury, and Billing functionalities.
3 - Add a procedure/function + trigger to prevent approving of an order without full payment if the Customer's ReliabilityStatus is 0 (Unreliable)

.
.
.

10 - Add Authorization, and Authentication!
    10.1 - Customers:
        + Can sign-up and sign-in
        + Update/Change Name, Email, Phone, Password
        + They can see the fabrics in stock and
        + Place a purchase order by choosing at least 1 fabric from the available fabric list shown to them
        + Place a supply order by choosing at least 1 fabric to supply (We might add a list of fabrics we prefer. Like the ones with low stock)
    10.2 - Employees:
        + Can NOT sign-up themselves! They need to be created by another employee, like maybe an admin. They can sign-in after they are recorded into the system
        + All employees can update/change their own Email, Phone, Password
        + Some, upper AccessLevel employees can modify their own and some other's Name, EmployeeRole, AccessLevel in addition to the ones above
        + Approve or Cancel orders
        + Update orders, shipping, reliability status of customers etc.