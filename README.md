# SFA Application (ASP.NET Core + MySQL)

Initial implementation includes:
- Authentication and role setup with ASP.NET Core Identity.
- Core modules: Customers, Products, Visits, Orders, Approvals.
- Entity Framework Core migrations configured for MySQL.

## Tech Stack
- .NET 9 MVC
- EF Core 9 + Pomelo MySQL provider
- ASP.NET Core Identity

## Project Structure
- `SfaApp.sln`
- `SfaApp.Web/`

## Local Setup
1. Update the MySQL connection string in:
   - `SfaApp.Web/appsettings.Development.json`
2. Create the database user with required privileges.
3. Apply migrations:

```powershell
cd "C:\Users\rstra\Desktop\Rajat Application\SFA"
dotnet ef database update --project .\SfaApp.Web\SfaApp.Web.csproj --startup-project .\SfaApp.Web\SfaApp.Web.csproj
```

4. Run application:

```powershell
cd "C:\Users\rstra\Desktop\Rajat Application\SFA\SfaApp.Web"
dotnet run
```

## Seeded Access
- Roles: `Admin`, `Manager`, `SalesRep`
- Admin user:
  - Email: `admin@sfa.local`
  - Password: `Admin@12345`

Change seeded credentials before production use.
