# SfaApp.Api

Node.js + Express REST API for SFA workflows with:
- MySQL persistence (required)
- JWT authentication
- Offline sync ingestion endpoint
- Object storage integration for uploads/reports (local or S3-compatible)
- Optional Redis cache

## Quick Start

1. Install dependencies:

```powershell
cd "C:\Users\rstra\Desktop\Rajat Application\SFA\SfaApp.Api"
npm install
```

2. Configure environment:

```powershell
Copy-Item .env.example .env
```

3. Start API:

```powershell
npm run dev
```

## Default API Login

- Username: `apiadmin`
- Password: `Admin@12345`

Change it in database for production.

## Key Endpoints

- `POST /api/auth/login`
- `GET /api/health`
- `POST /api/mobile/sync/push` (JWT required)
- `POST /api/files/upload` (JWT required)
- `GET /api/files/:fileId/download` (JWT required)
- `GET /api/reports/daily?date=YYYY-MM-DD` (JWT required)