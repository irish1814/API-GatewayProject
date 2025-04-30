
# API Gateway

An API Gateway that communicates with Crypto-Currencies services and enable to have info, buy and sell Crypto-Currencies


## Installation

Install Packages

```bash
dotnet add package Microsoft.EntityFrameworkCore -version 8.0.1
dotnet add package Microsoft.EntityFrameworkCore -version 8.0.1
dotnet add package Microsoft.EntityFrameworkCore.Tools -version 8.0.1
dotnet add package Pomelo.EntityFrameworkCore.MySql -version 8.0.1
```

To connect to the database run the following commands in the Developer Cmd

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate
dotnet ef database update
```
## Connect the Database

Open appsettings.json file and add the following
```bash
    "ConnectionStrings": {
    "<YourConnetionStringName": "server=SQLServerIP;port=SQLServerPort;database=SQLServerDatabaseName;user=DatabaseUsername;password=DatabasePassword"
  },
```


## Authors

- [@irish1814](https://www.github.com/irish1814)
- [@Elior-S](https://www.github.com/Elior-S)

