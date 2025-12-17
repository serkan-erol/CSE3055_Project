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
4 - CTRL+Left Click on the http://localhost:3055 link on the top `info:` label or open your browser and go to: http://localhost:3055/swagger/index.html 
    4.1 - You will see the SwaggerUI. This page let's you use the defined end-points of the API 
    4.2 - From here try to add a customer or an employee. Try to delete or modify them and check if you can see the changes in the DB itself
    4.3 - When you have a Customer, try creating an Order for that Customer
    4.4 - When you have an Employee, try approving, and updating that Order
    4.5 - Test stuff like Locking an Order entry, test if it updated after being locked etc.