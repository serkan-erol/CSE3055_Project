# CSE3055_Project

1 - Download the repo. Preferably, clone it directly into your IDE
2 - Find the SQL code Tables_Combined_v1.sql for the DB in Kismet.DataAccess\Database\
  2.1 - In MS SQL Server Management Studio, create a new DB, and name is KismetDB
  2.2 - Run the entire query inside the Tables_Combined_v1.sql to create the tables for the DB
3 - Open up a terminal in the main project folder. Again, preferably your IDE's terminal for ease of use
  3.1 - Run the following commands:
    dotnet build
    cd .\Kismet.API\
    dotnet run
  Now the back-end is running
4 - Open your browser and go to: http://localhost:3055/swagger/index.html
  4.1 - You will see the SwaggerUI. That let's you use the defined end-points of the API
  4.2 - From here try to add a customer or an employee. Try to delete or modify them and check if you can see the changes in the DB itself
