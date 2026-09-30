# Customer Management System

A web-based customer management application built with ASP.NET Core MVC, Entity Framework Core, and SQL Server.

## Features

- **User Authentication** — Register, login, logout, and profile management (change password, edit profile)
- **Role-Based Access Control**
  - **Admin** — View all customers, manage users, assign roles
  - **User** — View and manage only their own customer data
- **Customer Management (CRUD)** — Create, read, update, and delete customer records
- **Customer Details** — Store and manage detailed information for each customer
- **Soft Delete** — Deleted records are retained in the database (marked, not removed)
- **Audit Trail** — Track who created and updated records, with timestamps
- **Responsive UI** — Built with Bootstrap 5

## Technologies

- .NET 10 / ASP.NET Core MVC
- Entity Framework Core 10
- SQL Server
- ASP.NET Core Identity
- Bootstrap

## Project Structure

```text
├── Controllers/
│   ├── AccountController.cs      # Auth (login, register, logout, profile)
│   ├── CustomerController.cs     # Customer CRUD
│   ├── CustomerDetailController.cs
│   ├── HomeController.cs
│   └── UserManagementController.cs
├── Data/
│   └── AppDbContext.cs           # EF Core + Identity DbContext
├── Models/
│   ├── Customer.cs
│   ├── CustomerDetail.cs
│   └── ErrorViewModel.cs
├── ViewModels/
├── Views/
│   ├── Account/
│   ├── Customers/
│   ├── CustomerDetail/
│   ├── Home/
│   ├── Shared/
│   └── UserManagement/
├── wwwroot/                      # Static files (CSS, JS, lib, uploads)
├── Migrations/
├── appsettings.json
└── Program.cs
```

## Prerequisites

- .NET 10 SDK
- SQL Server (LocalDB or SQL Server Express)

## Setup

1. **Clone and restore**

   ```bash
   dotnet restore
   ```

2. **Configure the database connection** (optional)

   Edit `appsettings.json` and update the `DefaultConnection` string if needed.
   The default uses a local SQL Server Express instance:

   ```text
   Server=localhost\SQLEXPRESS;Database=CustomerManagementDb;Trusted_Connection=True;TrustServerCertificate=True;
   ```

3. **Apply migrations**

   ```bash
   dotnet ef database update
   ```

4. **Run the application**

   ```bash
   dotnet run
   ```

   The app starts on `http://localhost:5258` and/or `https://localhost:7061`.

## Usage

1. Navigate to the application in your browser.
2. **Register** a new account or **Login** with existing credentials.
3. The first registered user can be assigned the **Admin** role (via the database or a setup script) to access admin features.
4. As an **Admin**, you can manage all customers and users, and assign roles.
5. As a regular **User**, you can view and manage only the customers you created.

## License

See [LICENSE.txt](LICENSE.txt).
