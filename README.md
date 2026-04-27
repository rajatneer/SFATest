# SFA Application (ASP.NET Core + MySQL)

Initial implementation includes:
- Authentication and role setup with ASP.NET Core Identity.
- Core modules: Customers, Products, Visits, Orders, Approvals.
- Entity Framework Core migrations configured for MySQL.

## Tech Stack
- .NET 9 MVC
- EF Core 9 + Pomelo MySQL provider
- ASP.NET Core Identity
- React + Vite PWA frontend in `Review and Design Application/`
- Node.js + Express REST API in `SfaApp.Api/`
- MySQL for web and API persistence
- Object storage support (local or S3-compatible) for uploads/reports
- Optional Redis cache support for API health/cache layer

## Project Structure
- `SfaApp.sln`
- `SfaApp.Web/`
- `SfaApp.Api/`
- `Review and Design Application/`

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

5. Run REST API (Node + Express):

```powershell
cd "C:\Users\rstra\Desktop\Rajat Application\SFA\SfaApp.Api"
Copy-Item .env.example .env
npm install
npm run dev
```

6. Run React PWA frontend:

```powershell
cd "C:\Users\rstra\Desktop\Rajat Application\SFA\Review and Design Application"
Copy-Item .env.example .env
npm install
npm run dev
```

## Seeded Access
- Roles: `Admin`, `Manager`, `SalesRep`
- Admin user:
  - Email: `admin@sfa.local`
  - Password: `Admin@12345`

Change seeded credentials before production use.
