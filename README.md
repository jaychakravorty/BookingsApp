# BookingsApp

How to setup
1. Create A DB Server and DB and add the db Connection at BookingsApp/appsettings.json under DefaultConnection.
2. Goto BookingApp.Initializer.Console folder and execute `dotnet run`. The console app will clear all data on tables to do a clean setup. Add the Members.csv and Inventories.csv provided under SetupData.
3. Goto BookingsApp and execute `dotnet run` to bring up the API, Get All member information through member endpoint and Get All Inventories through inventory endpoint, use the ids to call the `/book` endpoint and `/cancel` endpoint.
