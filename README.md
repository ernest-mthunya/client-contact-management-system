# Client Contact Management

## 1. Clone the Repository

```bash
git clone https://github.com/ernest-mthunya/client-contact-management-system.git
cd client-contact-management
```

## 2. Restore Dependencies

```bash
dotnet restore
```

## 3. Configure the Database

Open `appsettings.json` and update the connection string to point to your SQL Server instance:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ClientContactDb;Trusted_Connection=True;"
  }
}
```

> For a named SQL Server instance replace `(localdb)\\mssqllocaldb` with your server name:
> `Server=YOUR_SERVER_NAME;Database=ClientContactDb;Trusted_Connection=True;`

## 4. Apply Migrations

```bash
dotnet ef database update
```

If `dotnet ef` is not recognised, install the EF Core CLI tools first:

```bash
dotnet tool install --global dotnet-ef
dotnet ef database update
```

## 5. Run the Application

```bash
dotnet run
```

The app will be available at:

```
https://localhost:5001
http://localhost:5000
```

Or open the solution in Visual Studio and press **F5**.
