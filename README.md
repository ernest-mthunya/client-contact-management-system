#Client Contact Management
#A .NET MVC Core application for managing clients and contacts with many-to-many relationships.

Prerequisites
Make sure you have the following installed before getting started:

.NET 8 SDK
.SQL Server or SQL Server LocalDB
.Git

1. Clone the Repository

git clone https://github.com/ernest-mthunya/client-contact-management.git
cd client-contact-management

2. Configure the Connection String
Open appsettings.json and update the DefaultConnection to point to your SQL Server instance:

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ClientContactDb;Trusted_Connection=True;"
  }
}

For a named SQL Server instance replace (localdb)\\mssqllocaldb with Server=YOUR_SERVER_NAME;Database=ClientContactDb;Trusted_Connection=True;

3. Restore Dependencies
dotnet restore

This will download all NuGet packages defined in the .csproj file including:

Microsoft.EntityFrameworkCore
Microsoft.EntityFrameworkCore.SqlServer
Microsoft.EntityFrameworkCore.Tools

4. Run Migrations
Apply all pending migrations to create the database schema:
dotnet ef database update

5. Run the Application

dotnet run


client-contact-management/
├── Controllers/
│   ├── ClientController.cs
│   └── ContactController.cs
├── Data/
│   └── ClientContactManagementDbContext.cs
├── Entities/
│   ├── Client.cs
│   ├── Contact.cs
│   └── ClientContact.cs
├── Migrations/
├── Models/
│   ├── ClientRequest.cs
│   ├── ClientResponse.cs
│   ├── ContactRequest.cs
│   └── ContactResponse.cs
├── Services/
│   ├── ICrudService.cs
│   ├── IClientService.cs
│   ├── IContactService.cs
│   ├── ClientService.cs
│   ├── ContactService.cs
│   └── ClientCodeService.cs
├── Views/
│   ├── Client/
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   └── Delete.cshtml
│   └── Contact/
│       ├── Index.cshtml
│       ├── Create.cshtml
│       ├── Edit.cshtml
│       └── Delete.cshtml
├── appsettings.json
├── Program.cs
└── README.md






