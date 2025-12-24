# CSE3055_Project

    1 - Download the repo. Preferably, clone it directly into your IDE 
    
    2 - Find the SQL code Tables_and_FTPV_Combined_v(latest_version).sql for the DB in Kismet.DataAccess\Database
        2.1 - In MS SQL Server Management Studio, open a new query
        2.2 - Run the entire query inside the Tables_and_FTPV_Combined_vX.sql to create the DB and the tables for the DB 
        
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

    5 - Now we have an initial front-end!
        5.1 - Open up another terminal while the first one is still running the back-end
        5.2 - Run the following commands:

            cd .\KismetFrontend
            npm install
            npm audit fix --force
            npm run dev

        Make sure you are in the KismetFrontend folder! That first command is important! 

        5.3 - When you see the link with the localhost in the terminal, CTRL + Left Click. 
    Here you will see our webapp/UI. Now you do not have to deal with API end-points. Well...
    For some of the things, at least.
        5.4 - For now, Creating/Adding a Customer (no Employee creation, yet), login, logout, 
    updating stuff like email, password etc are implemented for both Customer and Employee
        5.5 - Go, test it!
