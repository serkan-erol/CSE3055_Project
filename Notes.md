# CSE3055_Project_TODO_Notes

    DONE 1 - Keep adding triggers for updating the LastUpdatedAt fields of tables. 
        1.1 - This takes too much space, and creates too much duplicate code. 
        Maybe, we should just send "LastupdatedAt = sysdatetime()" with queries that require it?

    DONE 2 - Add everything for FinancialTransaction, Payment, Treasury, 
    Billing, Shipment, Fabric, and Batch functionalities.

    DONE 3 - Add a procedure/function + trigger to prevent approving of an order without 
    full payment of previous billings + TotalDue of current order, if the Customer's 
    ReliabilityStatus is 0 (Unreliable)
        CANCEL 3.1 - We might auto-approve reliable customers' purchase orders 
        (We may change the deafult ReliabilityStatus to 0 (Unreliable))
        3.2 - Supply orders always require employee approval but it does not matter
        if the Customer is reliable or not

    DONE 4 - If a FT's TransactionDate is later then Customer's last suitable(*) Billing entry's 
    BillingDate, we will create a new Billing entry. If it is earlier, we will update the current/last 
    Billing entry with the new transaction entry

        * : Billing entry with the same BillingType as FT's TransactionType and 
    a later BillingDate then TransactionDate of the FT entry is a suitable entry to update, 
    instead of creating a new Billing entry for each new FT entry

            - An edge case : Customer owes us 50k, sells us 50k worth of fabric. Logically, we are even. 
        However, now we have either 2 Billing entries with no associated Payment entries. Purhcase FT is not paid, 
        and we did not pay the Supply FT. How are we going to deal with this?

    DONE 5 - Add a trigger to auto update a FT's TotalPaid when a Payment with that FT's FK is inserted/deleted

    DONE 6 - When a Customer adds a bunch of different Fabrics and clicks to the create order button, 
        6.1 - Create an order with 0 total amount. 
        6.2 - Send the chosen fabric infos along with this Order's OrderID and save the Batches to the DB.
        6.3 - Let the 'dbo.trg_UpdateOrderTotal' to calculate Order's TotalAmount.
        6.4 - After the Order's TotalAmount is updated, create the related FT, and Billing entries.
        6.5 - When the order is approved, create the corresponding Shipment entry/entries.

    DONE 7 -  Re-arrange some repositories and controllers. Also, delete the connection between Payment and Billing.
        DONE 7.1 - Batch: See all the Batch entries in a Shipment. 
        Add: GetBatchByShipmentIdAsync
        
        DONE 7.2 - Billing: An Employee should be able to see all of a Customer's Billing entries.
        Delete: GET/api/Billing/all-billings/for-employees and
        Add: GET/api/Billing/{customerId}/all-billings/for-employees

        DONE 7.3 - Customer:
        Add: End-point for GetByEmailAsync
        Add: Ability to GetByCustomerNumber
        
        DONE 7.4 - Employee: 
        Add: End-point for GetByEmailAsync
        Add: Ability to GetByEmployeeNumber
        
        DONE 7.5 - Fabric: PATCH/api/Fabric/{fabricId}/stock should not ask for FabricID twice.
        Make it [JsonIgnore in the DTO] and take the FabricID provided by router.

        DONE 7.6 - FT: An Employee should be able to see all of a Customer's FT entries.
        Add: GET/api/FinancialTransaction/{customerId}/get-all-financial-transactions/for-employees

        DONE 7.7 - Order: Implement ability to cancel an Order
        Delete: GET/api/Order/all-orders/for-employees
        Add: End-point for cancelling an Order. Use the existing UpdateOrderStatusAsync if possible.
        Add: GetByOrderNumber

        DONE 7.8 - Shipment: Update Origin and Destination country columns based on the OrderType.
        If the order is Purchase type, Origin Türkiye, destination Customer's Country if exists.
        If the order is Supply type, reverse it.
        Delete: POST/api/Shipment/{shipmentId}/lock Triggers already handle the locking of a shipment entry.
        Add: GetDisplayName for GET/api/Shipment/{shipmentId}/status/display

        DONE 7.9 - Payment: Cut the ties with Billing!!!        
    
    DONE 8 - When the 7 is completed, add more functionality to the front-end for employees.

    9 - Update the dbo.trg_Delete_FT_On_Order_Cancelled trigger to re-calculate TotalAmount and TotalPaid based on completed Payments if the order is cancelled

    10 - Add a trigger to mark an Order as Cancelled (4) if its Shipments are ALL failed (3)
        10.1 - Add a trigger to re-calculate an Order's TotalAmount (and therefore creating an update wave) if at least
        one of its Shiptment's has failed but not all

    11 - Customers can add card and bank info and delete them but can not modify currently. Improve it.

    .
    .
    .

    100 - Add Authorization, and Authentication!
        DONE 100.1 - Customers:
            DONE + Can register and login
            DONE + Update/Change Name, Email, Phone, Password
            DONE + They can see the fabrics in stock and
            DONE + Place a purchase order by choosing at least 1 fabric from the available fabric list shown to them
            DONE + Place a supply order by choosing at least 1 fabric to supply (We might add a list of fabrics we prefer. 
            Like the ones with low stock)

        100.2 - Employees:
            DONE + Can NOT register themselves! They need to be created by another employee, like maybe an admin. 
            DONE + They can login after they are recorded into the system
            DONE + All employees can update/change their own Email, Phone, Password
            DONE + Higher AccessLevel employees can modify EmployeeRole, and AccessLevel of other Employees
            DONE + Approve or Cancel orders.
            PARTIALLY DONE + Update orders, shipping, reliability status of customers etc.
