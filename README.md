
# API Gateway

An API Gateway that communicates with Crypto-Currencies services and enable to have info, buy and sell Crypto-Currencies

## Deploy Project

Clone the project

```bash
git clone https://github.com/irish1814/API-GatewayProject.git
```

Go to the project directory

```bash
cd API-GatewayProject/ApiGateway
```

## Install server dependencies

Install Packages

```bash
dotnet add package Microsoft.EntityFrameworkCore -version 8.0.1
dotnet add package Microsoft.EntityFrameworkCore -version 8.0.1
dotnet add package Microsoft.EntityFrameworkCore.Tools -version 8.0.1
dotnet add package Pomelo.EntityFrameworkCore.MySql -version 8.0.1
dotnet add package Swashbuckle.AspNetCore
dotnet add package Microsoft.Extensions.AI.Ollama --prerelease
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

## Set up databases
Copy and past the follwoing code to a docker-compose.yaml file, and run it using docker-compose up -d in a server or your own machine

```yaml
services:
    mysql:
        image: ubuntu/mysql
        
        container_name: ApiServerMysqlDb
        
        environment:
            - MYSQL_ROOT_PASSWORD_FILE=/run/secrets/db_root_password
            - MYSQL_USER=user
            - MYSQL_PASSWORD_FILE=/run/secrets/db_password
            - MYSQL_DATABASE=ApiServerDb
            - TZ=Asia/Jerusalem
        
        secrets:
          - db_password 
          - db_root_password

        volumes:
            - ApiServerDBVolume=/var/lib/mysql
        
        hostname: mysql

        ports:
            - 3306:3306
        
        networks:
          ApiServerNetwork:
              ipv4_address: 10.10.10.2

        restart: unless-stopped

    redis:
        image: ubuntu/redis
        container_name: ApiServerRedisCache
        
        # command: 'redis-server --requirepass password --save 60 1 --loglevel warning'
        
        networks:
          ApiServerNetwork:
            ipv4_address: 10.10.10.3

        ports:
            - '6379:6379'
        
        volumes:
            - ApiServerCacheVolume:/data

        secrets:
          - cache_password 

        environment:
          - TZ=Asia/Jerusalem
          - REDIS_PASSWORD=/run/secrets/cache_password

        restart: unless-stopped

volumes:
  ApiServerDBVolume:
    name: ApiServerDBVolume
  ApiServerCacheVolume:
    name: ApiServerCacheVolume

secrets:
   db_password:
     file: ./config/db_password
   db_root_password:
     file: ./config/db_root_password
   cache_password:
     file: ./config/redis_password

networks:
    ApiServerNetwork:
      name: ApiServerNetwork
      ipam:
        driver: default 
        config:
          - subnet: "10.10.10.0/24"
```

## Connect to databases
To connect to the databases run the following commands in the Developer Cmd

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Open appsettings.json file and add the following
```bash
    "ConnectionStrings": {
    "<MySQLDatabaseConnectionString": "server=<SQLServerIP>;port=<SQLServerPort>;database=<SQLServerDatabaseName>;user=<DatabaseUsername>;password=<DatabasePassword>"
    "<RedisDatabaseConnectionString": "<RedisServerIP>:RedisServerPort,password=<RedisDBPassword>"
  },
```

## Connect to Ollama Model
Update line 47 in APiServicesController file with your settings
```bash
OllamaChatClient chatClient = new OllamaChatClient(endpoint: new Uri("<OllamaServerIP>"), modelId: "<ModelName:Tag>");
```

## Install client dependencies

Go to the client project directory

```bash
cd API-GatewayProject/ApiGateway/GUI-Client
```

Install Packages
```bash
python -m venv .venv
.\.venv\Scripts\activate.ps1
pip install -r .\requirements.txt
```

# Start the server
```bash
dotnet run .\API-GatewayProject\ApiGateway.csproj
```
# Start the client
```bash
python .\LoginWindow.py
```
## Authors

- [@irish1814](https://www.github.com/irish1814)
- [@Elior-S](https://www.github.com/Elior-S)
