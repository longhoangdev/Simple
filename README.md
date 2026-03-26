# SimpleApp

A .NET 9.0 web application with PostgreSQL, Redis, RabbitMQ (event-driven messaging), Azure AD B2C authentication, and structured logging via Serilog.

## Prerequisites

- .NET 9.0 SDK
- Docker & Docker Compose
- PostgreSQL (for local development)
- Redis (for local development)
- RabbitMQ (for local development)

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
   - RabbitMQ connection settings

### 2. Running with Docker

The easiest way to run the application with all dependencies:

```powershell
cd SimpleApp/Docker
docker-compose up --build
```

The application will be available at: `http://localhost:8081`  
RabbitMQ Management UI: `http://localhost:15672` (guest / guest)

### 3. Running Locally (Development)

1. Start PostgreSQL, Redis and RabbitMQ (via Docker):
   ```powershell
   cd SimpleApp/Docker
   docker-compose up postgres redis rabbitmq
   ```

2. Run the application:
   ```powershell
   cd SimpleApp
   dotnet run
   ```

### 4. Database Migrations

Apply migrations to create/update the database schema (includes Outbox tables):

```powershell
cd SimpleApp
dotnet ef database update
```

## Project Structure

- **Api/** - API endpoints and endpoint builders
- **Domain/** - Domain entities, interfaces, and **domain events**
- **Persistence/** - Database context, configurations, and migrations
- **Shared/** - Shared utilities (caching, messaging, extensions, etc.)
- **Docker/** - Dockerfile, docker-compose, and .dockerignore

## Architecture

- **CQRS Pattern** - Using MediatR for command/query separation
- **Vertical Slice Architecture** - Features organized by functionality
- **Event-Driven Architecture** - Domain events published via MassTransit + RabbitMQ
- **Transactional Outbox Pattern** - Events stored in PostgreSQL within the same DB transaction (no dual-write problem)
- **Repository Pattern** - Entity Framework Core for data access
- **Caching** - Redis for distributed caching
- **Authentication** - Azure AD B2C with JWT Bearer tokens
- **Logging** - Serilog with Console and File sinks

## Event-Driven Messaging

This project uses [MassTransit](https://masstransit.io/) with RabbitMQ as the message broker.

### Domain Events

| Event | Published When | Consumer Action |
|---|---|---|
| `UserCreatedEvent` | User is created | Welcome email stub (logged via Serilog) |
| `UserUpdatedEvent` | User profile is updated | Audit log entry |
| `UserDeletedEvent` | User is soft-deleted | Audit log entry |

### Patterns Used

| Pattern | Implementation |
|---|---|
| **Transactional Outbox** | `MassTransit.EntityFrameworkCore` — events are committed to PostgreSQL in the same transaction as the domain write, then delivered asynchronously |
| **Dead-Letter Queue** | Each consumer has a `ConsumerDefinition` that auto-creates a `<queue>_error` queue for undeliverable messages |
| **Retry Policy** | `UseMessageRetry(r => r.Intervals(500ms, 1s, 5s))` per consumer |
| **Idempotency** | MassTransit `InboxState` table deduplicates redelivered messages automatically |

### Queue Names

- `user-created` / `user-created_error`
- `user-updated` / `user-updated_error`
- `user-deleted` / `user-deleted_error`

## Logging

Structured logging is handled by [Serilog](https://serilog.net/).

| Environment | Level | Sinks |
|---|---|---|
| Development | Debug | Console + File (`logs/`) |
| Production | Information | Console + File (`logs/`) |

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

