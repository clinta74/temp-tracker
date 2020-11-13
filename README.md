# Temp Tracker Service
ASP.NET Core 3.1 Web API using EF Core 3 to connect to a database.  Used to track the reading of
temperatures to a facility.

## Developement
Add user secrets
```
dotnet user-secrets set DB_USERNAME <db_username>
dotnet user-secrets set DB_PASSWORD <db_password>
dotnet user-secrets set JWT_KEY <jwt_key>
```
Run the application
```
dotnet watch run
```

### Environment
```
DB_USERNAME <username>
DB_PASSWORD <password>
JWT_KEY <hash key>
```