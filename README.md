# CSE3055_Project

    1 - Download the repo. Preferably, clone it directly into your IDE 
    
    2 - Find the SQL code Tables_and_IPTIV_Combined_v(latest_version).sql 
    for the DB in Kismet.DataAccess\Database
    
        2.1 - In MS SQL Server Management Studio, open a new query
        
        2.2 - Run the entire query inside the Tables_and_IPTIV_Combined_vX.sql 
        to create the DB and the tables for the DB 
        
    3 - Open up a terminal in the main project folder. Again, preferably your IDE's terminal for ease of use 
        3.1 - Run the following commands: 
        
            dotnet build 
            cd .\Kismet.API
            dotnet run 
            
    When you see 4 green `info:` labels the back-end is running 
    
    4 - CTRL+Left Click on the http://localhost:3055 link on the top `info:` label 
    or open your browser and go to: http://localhost:3055/swagger/index.html 
    
        4.1 - You will see the SwaggerUI. This page let's you use the defined end-points of the API 
        
        4.2 - From here try to add a customer or an employee. Try to delete or modify them 
        and check if you can see the changes in the DB itself
        
        4.3 - When you have a Customer, try creating an Order for that Customer
        
        4.4 - When you have an Employee, try approving, and updating that Order
        
        4.5 - Test stuff like locking an Order entry, test if it still updates after being locked etc.

        4.6 - Now the application has all the functionality. Test-on!

    5 - Now we have an front-end!
    
        5.1 - Open up another terminal while the first one is still running the back-end
        
        5.2 - Run the following commands:

            cd .\KismetFrontend
            npm install
            npm audit fix --force
            npm run dev

        Make sure you are in the KismetFrontend folder! That first command is important! 

        5.3 - When you see the link with the localhost in the terminal, CTRL + Left Click. 
        Here you will see our webapp/UI. Now you do not have to deal with API end-points. Well...
        For most of the things, at least.
        
    6 - Implemented features to test:

        6.1 - Customers can register, login, logout.

        6.2 - Creating/Adding a new Employee (only employees with AccessLevel 7 or above can do this)
        
        6.3 - Updating stuff like email, password, phone no etc are implemented for both Customer and Employee

        6.4 - Employees are able to view specific pages and are able to perform specific tasks 
        according to their AccessLevel. 
        For example; a sales employee (AccessLevel 3) can not even see the ManageEmployee page. 
        Meanwhile, a local_admin (5) can see the page, and they can see all of the employees but that is it.
        They can not perform any action. An admin (7) can update Roles and AccessLevels for employees 
        that has a lower AccessLevel than themselves.
        Another example; only accountants (4) and sys_admin (9) or above can see the entire Treasury table
        at once. Meanwhile, anyone that is not an accountant (4), sys_admin (9) or a C-level executive (10)
        can not see it. Even if they can see the Treasury page and even if they have a role with higher 
        AccessLevel than an accountant (4), like local_admin(5)

        6.5 - Almost all of the functionality is implemented. 
            - Customers can place Orders
            - FT entry is automatically created when an Order is placed
            - Either a new Billing entry is created or a suitable one is updated with the new FT's info
            - Employees can approve or cancel that order. They can update Order's status to shipped etc.
            - Customers can make payments for their purchase orders.
            - Customers and Employees can view a wide-range of information in the corresponding pages 
            provided to them.
            - Employees can update most of the tables.
            - And many more...

        6.6 - Missing front-end functionality
            - Making payments to Customers for their Supply type orders. We can do it from API end-points.
            - ManageCustomer page. Updating ReliabilityStatus of a Customer is currently done via API.

    7 - Go, test it!
