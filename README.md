# SimpleApp

A .NET 9.0 web application with PostgreSQL database, Redis caching, Azure AD B2C authentication, and structured logging via Serilog.

## Prerequisites

- .NET 9.0 SDK
- Docker & Docker Compose
- PostgreSQL (for local development)
- Redis (for local development)

## Setup Instructions

### 1. Configuration Files

⚠️ **IMPORTANT**: This project uses sensitive configuration files that are NOT committed to Git.

1. Copy the example file to create your local configuration:
   ```powershell
   Copy-Item SimpleApp\appsettings.Development.json.example SimpleApp\appsettings.Development.json
   ```

2. Edit `SimpleApp\appsettings.Development.json` and replace the placeholder values:
   - Database connection string (PostgreSQL credentials)
   - Redis connection string and password
   - Azure AD B2C settings (Instance, ClientId, Domain, etc.)

### 2. Running with Docker

The easiest way to run the application with all dependencies:

```powershell
cd SimpleApp
docker-compose up --build
```

The application will be available at: `http://localhost:8081`

### 3. Running Locally (Development)

1. Start PostgreSQL and Redis (via Docker):
   ```powershell
   cd SimpleApp
   docker-compose up postgres redis
   ```

2. Run the application:
   ```powershell
   cd SimpleApp
   dotnet run
   ```

### 4. Database Migrations

Apply migrations to create/update the database schema:

```powershell
cd SimpleApp
dotnet ef database update
```

## Project Structure

- **Api/** - API endpoints and endpoint builders
- **Domain/** - Domain entities and interfaces
- **Persistence/** - Database context, configurations, and migrations
- **Shared/** - Shared utilities (caching, messaging, extensions, etc.)

## Architecture

- **CQRS Pattern** - Using MediatR for command/query separation
- **Vertical Slice Architecture** - Features organized by functionality
- **Repository Pattern** - Entity Framework Core for data access
- **Caching** - Redis for distributed caching
- **Authentication** - Azure AD B2C with JWT Bearer tokens
- **Logging** - Serilog with Console and File sinks

## Logging

Structured logging is handled by [Serilog](https://serilog.net/).

| Environment | Level   | Sinks                          |
|-------------|---------|--------------------------------|
| Development | Debug   | Console + File (`logs/`)       |
| Production  | Information | Console + File (`logs/`) |

Log files are written to `logs/app-{date}.log` (rolls daily) and are excluded from Git.

## Security Notes

🔒 **Never commit sensitive configuration files:**
- `appsettings.Development.json`
- `appsettings.Production.json`
- `.env` files

These files contain credentials and are ignored by Git. Always use the `.example` template files.

See [SECURITY_SETUP.md](./SECURITY_SETUP.md) for full details.

## Future Improvements

- [ ] Unit & integration tests (xUnit + Moq)
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Azure deployment

