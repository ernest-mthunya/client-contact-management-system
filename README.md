Client Contact Management

A .NET 8 ASP.NET Core MVC application for managing clients and contacts with a many-to-many relationship.
The system allows users to create clients, manage contacts, and link multiple contacts to multiple clients.

This project demonstrates clean architecture practices, Entity Framework Core, and a service-based architecture for maintainable and scalable applications.

Features

Create, update, and delete Clients

Create, update, and delete Contacts

Link multiple contacts to multiple clients

Automatically generate unique client codes

AJAX-based operations for a smooth UI experience

Entity Framework Core with migrations

Clean service-layer architecture

Technology Stack

.NET 8

ASP.NET Core MVC

Entity Framework Core

SQL Server / SQL Server LocalDB

Bootstrap

JavaScript (Fetch API for AJAX)

Prerequisites

Ensure the following tools are installed before running the project:

.NET 8 SDK

SQL Server or SQL Server LocalDB

Git

Getting Started
1. Clone the Repository
git clone https://github.com/ernest-mthunya/client-contact-management.git
cd client-contact-management
2. Configure the Connection String

Open appsettings.json and update the DefaultConnection string to match your SQL Server instance.

Example using LocalDB:

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ClientContactDb;Trusted_Connection=True;"
  }
}

If you are using a named SQL Server instance, update it as follows:

Server=YOUR_SERVER_NAME;Database=ClientContactDb;Trusted_Connection=True;
3. Restore Dependencies

Run the following command to restore all NuGet packages:

dotnet restore

This will install packages including:

Microsoft.EntityFrameworkCore

Microsoft.EntityFrameworkCore.SqlServer

Microsoft.EntityFrameworkCore.Tools

4. Apply Database Migrations

Create the database and apply the schema using:

dotnet ef database update

This command will generate the ClientContactDb database and apply all migrations.

5. Run the Application

Start the application using:

dotnet run

The application will start and can be accessed through the browser using the provided local URL.

Project Structure
client-contact-management/
│
├── Controllers/
│   ├── ClientController.cs
│   └── ContactController.cs
│
├── Data/
│   └── ClientContactManagementDbContext.cs
│
├── Entities/
│   ├── Client.cs
│   ├── Contact.cs
│   └── ClientContact.cs
│
├── Migrations/
│
├── Models/
│   ├── ClientRequest.cs
│   ├── ClientResponse.cs
│   ├── ContactRequest.cs
│   └── ContactResponse.cs
│
├── Services/
│   ├── ICrudService.cs
│   ├── IClientService.cs
│   ├── IContactService.cs
│   ├── ClientService.cs
│   ├── ContactService.cs
│   └── ClientCodeService.cs
│
├── Views/
│   ├── Client/
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   └── Delete.cshtml
│   │
│   └── Contact/
│       ├── Index.cshtml
│       ├── Create.cshtml
│       ├── Edit.cshtml
│       └── Delete.cshtml
│
├── appsettings.json
├── Program.cs
└── README.md
Architecture Overview

The application follows a layered architecture:

Controllers

Handle HTTP requests and responses.

Services

Contain the business logic and abstract database operations from controllers.

Entities

Represent the database models.

Models

DTOs used for request and response mapping between the UI and services.

Data

Contains the Entity Framework DbContext used for database access.

Views

Razor views used to render the MVC UI.

Database Design

The application models a many-to-many relationship between Clients and Contacts.

Client
   │
   │
ClientContact (Join Table)
   │
   │
Contact

This allows:

A Client to have multiple Contacts

A Contact to belong to multiple Clients

Author

Ernest Mthunya

GitHub:
https://github.com/ernest-mthunya
