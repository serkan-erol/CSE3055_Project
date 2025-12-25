# CSE3055_Project_TODO_Notes

    DONE 1 - Keep adding triggers for updating the LastUpdatedAt fields of tables. 
        1.1 - This takes too much space, and creates too much duplicate code. 
        Maybe, we should just send "LastupdatedAt = sysdatetime()" with queries that require it?

    DONE 2 - Add everything for FinancialTransaction, Payment, Treasury, 
    Billing, Shipment, Fabric, and Batch functionalities.

    DONE 3 - Add a procedure/function + trigger to prevent approving of an order without 
    full payment of previous billings + TotalDue of current order, if the Customer's 
    ReliabilityStatus is 0 (Unreliable)
        3.1 - We might auto-approve reliable customers' purchase orders 
        (We may change the deafult ReliabilityStatus to 0 (Unreliable))
        3.2 - SUpply orders always require employee approval

    DONE 4 - If a FT's TransactionDate is later then Customer's last suitable(*) Billing entry's 
    BillingDate, we will create a new Billing entry. If it is earlier, we will update the current/last 
    Billing entry with the new transaction entry

        * : Billing entry with the same BillingType as FT's TransactionType and 
    a later BillingDate then TransactionDate of the FT entry is a suitable entry to update, 
    instead of creating a new Billing entry for each new FT entry

            - An edge case : Customer owes us 50k, sells us 50k worth of fabric. Logically, we are even. 
        However, now we have either 2 Billing entries with no associated Payment entries. Purhcase FT is not paid, 
        and we did not pay the Supply FT. How are we going to deal with this?

    DONE 5 - Add a trigger to auto update a FT's TotalPaid when a Payment with that FT's FK is inserted

    DONE 6 - When a Customer adds a bunch of different Fabrics and clicks to the create order button, 
        6.1 - Create an order with 0 total amount. 
        6.2 - Send the chosen fabric infos along with this Order's OrderID and save the Batches to the DB.
        6.3 - Let the 'dbo.trg_UpdateOrderTotal' to calculate Order's TotalAmount.
        6.4 - After the Order's TotalAmount is updated, create the related FT, and Billing entries.

    7 - Customers can add card and bank info but can not modify or delete currently. Improve it.

    8 -  Re-arrange some repositories and controllers.
        
        - Add the ability to see a Customer's all the entries in any table for an employee. End-point of this method must require 
        a customerId and an employeeId to check if that employee has the necessary priviliges.
        
        - Delete the end-points that let employees see entirety of some tables without requiring any parameter etc.
        They are quite unnecessarry
    
    9 - When the 8 is completed, add more functionality to the front-end for employees

    .
    .
    .

    100 - Add Authorization, and Authentication!
        DONE 100.1 - Customers:
            DONE + Can sign-up and sign-in
            DONE + Update/Change Name, Email, Phone, Password
            DONE + They can see the fabrics in stock and
            DONE + Place a purchase order by choosing at least 1 fabric from the available fabric list shown to them
            DONE + Place a supply order by choosing at least 1 fabric to supply (We might add a list of fabrics we prefer. 
            Like the ones with low stock)

        100.2 - Employees:
            + Can NOT sign-up themselves! They need to be created by another employee, like maybe an admin. 
            DONE +They can sign-in after they are recorded into the system
            DONE + All employees can update/change their own Email, Phone, Password
            + Some, upper AccessLevel employees can modify their own and some other's Name, EmployeeRole, 
            AccessLevel in addition to the ones above
            PARTIALLY DONE + Approve or Cancel orders. Currently, approving is workking, Cancel is not
            + Update orders, shipping, reliability status of customers etc.
