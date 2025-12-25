# CSE3055_Project_TODO_Notes
DONE ~1 - Keep adding triggers for updating the LastUpdatedAt fields of tables. 
    1.1 - This takes too much space, and creates too much duplicate code. 
    Maybe, we should just send "LastupdatedAt = sysdatetime()" with queries that require it?~

2 - Add everything for ~FinancialTransaction~ (DONE), ~Payment~ (DONE), Treasury, 
~Billing~ (DONE), Shipment, Fabric, and Batch functionalities.

3 - Add a procedure/function + trigger to prevent approving of an order without 
full payment of previous billings + TotalDue of current order, if the Customer's 
ReliabilityStatus is 0 (Unreliable)
    3.1 - We might auto-approve reliable customers' purchase orders 
    (We may change the deafult ReliabilityStatus to 0 (Unreliable))

    3.2 - SUpply orders always require employee approval

4 - If a FT's TransactionDate is later then Customer's last suitable(*) Billing entry's 
BillingDate, we will create a new Billing entry. If it is earlier, we will update the current/last 
Billing entry with the new transaction entry

    * : We need to consider how are we going to handle billing.
    ** I'm MOVING FORWARD WITH SECOND OPTION **

        - First option : We keep both Purchase and Supply type FTs in the same Billing entry
    When a new FT is added to an existing Billing entry, or an FT is updated upon a payment; 
    it may require a BillingType change.
            - An example : Customer buys from us 50k worth of fabric. 
            Then, sells us 60k worth of fabric. 
            Now we owe 10k to the Customer. Since we are holding both in the same Billing entry, 
            we need to update the BillingType from Purchase to Supply.

            - Another example : Customer buys from us 50k worth of fabric. Then, sells us 40k 
            worth of fabric. Then, tries to make a payment of 15k for the first purhcase order. 
            We have to prevent this payment because in total, they owe us only 10k.

        - Second option : We keep separate Billing entries for Purchase and Supply type FTs.
    In this case, handling each Billing entry will be easier; no BillingType updates are required. 
    However, while dealing with updates to Billing and FT, or auto-approving orders we have to 
    carefully calculate all the Billing entries a customer has.
            - An example : Customer owes us 50k, this is held in an entry of its own with the BillingType: 
            Purchase. Customer sells us 60k worth of fabric, 
            this requires another entry of BillingType: Supply. If this customer is marked as unreliable 
            and tries to place a new order costing 10k, either the current Billing entry with the Purchase type 
            is updated (TotalDue = TotalDue + 10k, which is 60k) or a new one created with TotalDue = 10k. 
            Considering all the Billing entries, in total, we are even. Then, the order should be approved. 
            However, if they place an order costing 11k, it should stay in the pending state until they pay us 1k.

        - An edge case : Customer owes us 50k, sells us 50k worth of fabric. Logically, we are even. 
        However, depending on our decision, now we have either
            a) 2 Billing entries with no associated Payment entries. Purhcase FT is not paid, 
            and we did not pay the Supply FT. Or,
            b) 1 Billing entry showing 0 TotalDue (or showing the TotalDue of Purchase FT/Order 
            and showing it as completely paid (TotalPaid = TotalDue) because of the Supply FT's/Order's TotalDue), 
            again with no associated Payment entries. How are we going to deal with this?

5 - Add a trigger to auto update a FT's TotalPaid when a Payment with that FT's FK is inserted

    6 - Make sure to create related Batch entries with every Order placed
        6.1 - Customer chooses fabrics, clicks on place order and a batch is created for each and every fabric selected

    .
    .
    .

100 - Add Authorization, and Authentication!
    100.1 - Customers:
        + Can sign-up and sign-in
        + Update/Change Name, Email, Phone, Password
        + They can see the fabrics in stock and
        + Place a purchase order by choosing at least 1 fabric from the available fabric list shown to them
        + Place a supply order by choosing at least 1 fabric to supply (We might add a list of fabrics we prefer. 
        Like the ones with low stock)

    100.2 - Employees:
        + Can NOT sign-up themselves! They need to be created by another employee, like maybe an admin. 
        They can sign-in after they are recorded into the system
        + All employees can update/change their own Email, Phone, Password
        + Some, upper AccessLevel employees can modify their own and some other's Name, EmployeeRole, 
        AccessLevel in addition to the ones above
        + Approve or Cancel orders
        + Update orders, shipping, reliability status of customers etc.