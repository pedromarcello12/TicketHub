# TicketHub

A distributed ticket sales platform built on a microservices architecture, designed as a portfolio project showcasing scalability, resilience, and modern software design practices (DDD, CQRS concepts, database-per-service, async messaging).

---

## What it does

Users browse events, reserve tickets within a **10-minute window**, complete payment, and receive an email notification. Administrators manage events and payment lifecycle.

**Full flow:**
1. Admin creates and publishes an event via **Eventos.Api**
2. Admin adds tickets to that event via **Ingressos.Api**
3. User authenticates via **Auth.Api** and receives a JWT
4. User reserves a ticket — a 10-minute TTL starts immediately
5. User creates a payment via **Pagamento.Api**
6. Admin approves (or rejects) the payment
7. **Notificacoes.Worker** picks up the status change event from RabbitMQ and emails the user

If the user does not pay within 10 minutes, the reservation is automatically released by a background worker and the ticket becomes available again.

---

## Architecture

The system is split into four independent services, each with its own database (database-per-service), plus a shared Auth service:

| Service | Port | Responsibility |
|---|---|---|
| **Auth.Api** | 5004 | JWT issuance, refresh tokens, service-to-service auth |
| **Eventos.Api** | 5001 | Event CRUD with state machine |
| **Ingressos.Api** | 5002 | Ticket reservation, TTL expiry, sale confirmation |
| **Pagamento.Api** | 5003 | Payment processing with Polly resilience |
| **Notificacoes.Worker** | — | Consumes RabbitMQ, sends email notifications |

Services communicate **synchronously** (HTTP with JWT + Polly retry/circuit-breaker) for data validation, and **asynchronously** (RabbitMQ) for integration events.

For full architecture diagrams (C4 Context, Container, Component), see [`docs/architecture.md`](docs/architecture.md).

Architecture decision records are in [`docs/adr/`](docs/adr/).

---

## Tech stack

**Backend**
- .NET 10 / ASP.NET Core
- Entity Framework Core (migrations, Fluent API mapping)
- Polly via `Microsoft.Extensions.Http.Resilience` (retry, circuit breaker, timeout)
- RabbitMQ + `RabbitMQ.Client` (async integration events)
- Redis (distributed cache, available to all services)
- JWT Bearer authentication with role-based authorization

**Observability**
- Serilog — structured logging with Console and Seq sinks
- OpenTelemetry — distributed tracing and metrics (ASP.NET Core + HTTP Client instrumentation)
- Seq — centralized log aggregation UI (http://localhost:5341)

**Infrastructure**
- Docker Compose — local orchestration of all services and dependencies
- GitHub Actions — CI pipeline (build + unit tests on every push/PR to `develop` and `master`)

---

## Domain model

### Events (`Eventos.Api`)
Events have a state machine: `Rascunho → Publicado → Encerrado | Cancelado`. Only Admins can create, publish, cancel, or close events.

### Tickets (`Ingressos.Api`)
Tickets belong to an event and transition through: `Disponivel → Reservado → Vendido | Cancelado`.

Key rule: a `Reservado` ticket has a `ReservadoAte` timestamp (UTC now + 10 minutes). A background worker (`LiberacaoReservaExpiradaWorker`) periodically queries for expired reservations and releases them back to `Disponivel`.

Business invariants are enforced in the domain aggregate (`Ingresso`), not in the application layer.

### Payments (`Pagamento.Api`)
Payments transition through: `Pendente → Aprovado | Recusado → Estornado`.

After each status transition, Pagamento.Api publishes a `PagamentoStatusAlteradoEvent` to RabbitMQ. The Notificacoes.Worker picks it up and emails the customer.

### Auth (`Auth.Api`)
Two user roles: `Administrador` and `Usuario`. Service-to-service calls use a dedicated `ServicoInterno` role — services obtain a short-lived JWT by POSTing to `/api/auth/servico-interno` with a shared secret, then cache and inject it automatically via `AuthTokenDelegatingHandler`.

---

## Project structure

```
TicketHub/
├── src/
│   ├── BuildingBlocks/
│   │   ├── TicketHub.Auth/          # JWT generation, validation, rate limiting, service-to-service auth
│   │   ├── TicketHub.Core/          # Base entity, shared exceptions
│   │   ├── TicketHub.MessageBus/    # RabbitMQ publisher, consumer base, integration events
│   │   └── TicketHub.Observabilidade/ # Serilog + OpenTelemetry configuration
│   └── Services/
│       ├── Auth/
│       │   ├── Auth.Api/
│       │   ├── Auth.Application/
│       │   ├── Auth.Domain/
│       │   └── Auth.Infrastructure/
│       ├── Eventos/
│       │   ├── Eventos.Api/
│       │   ├── Eventos.Application/
│       │   ├── Eventos.Domain/
│       │   └── Eventos.Infrastructure/
│       ├── Ingressos/
│       │   ├── Ingressos.Api/
│       │   ├── Ingressos.Application/
│       │   ├── Ingressos.Domain/
│       │   └── Ingressos.Infrastructure/
│       ├── Pagamento/
│       │   ├── Pagamento.Api/
│       │   ├── Pagamento.Application/
│       │   ├── Pagamento.Domain/
│       │   └── Pagamento.Infrastructure/
│       └── Notificacoes/
│           └── Notificacoes.Worker/
├── tests/
│   ├── Auth.Tests/
│   ├── Eventos.Tests/
│   ├── Ingressos.Tests/
│   └── Pagamento.Tests/
├── docs/
│   ├── architecture.md              # C4 diagrams (Context, Container, Component)
│   ├── adr/                         # Architecture Decision Records
│   ├── GIT_FLOW.md
│   └── SECRETS.md
├── docker-compose.yml
├── .env.example
└── TicketHub.slnx
```

---

## Running locally

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### 1. Configure secrets

```bash
cp .env.example .env
```

Edit `.env` and fill in `JWT_SECRET_KEY` and `SERVICO_INTERNO_SENHA` with strong random values:

```bash
# Generate a strong key (Linux/macOS)
openssl rand -base64 48
```

### 2. Start all services

```bash
docker compose up --build
```

This starts SQL Server, RabbitMQ, Redis, MailHog, Seq, and all five application services. On first startup, Auth.Api automatically runs EF Core migrations and seeds the admin user.

### 3. Services and tools

| Service | URL |
|---|---|
| Auth.Api | http://localhost:5004 |
| Eventos.Api | http://localhost:5001 |
| Ingressos.Api | http://localhost:5002 |
| Pagamento.Api | http://localhost:5003 |
| RabbitMQ Management | http://localhost:15672 (guest/guest) |
| MailHog (e-mail UI) | http://localhost:8025 |
| Seq (logs) | http://localhost:5341 |

### 4. Authenticate

```bash
# Login as admin (seeded on startup)
curl -X POST http://localhost:5004/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "admin@tickethub.com", "senha": "Admin@123"}'
```

Use the returned `token` as `Authorization: Bearer <token>` on subsequent requests.

### 5. OpenAPI docs

Each API service exposes OpenAPI in development mode:

- Auth.Api: http://localhost:5004/openapi/v1.json
- Eventos.Api: http://localhost:5001/openapi/v1.json
- Ingressos.Api: http://localhost:5002/openapi/v1.json
- Pagamento.Api: http://localhost:5003/openapi/v1.json

---

## Running tests

```bash
dotnet test TicketHub.slnx
```

Tests are organized per service and cover domain rules and application service logic.

---

## CI/CD

GitHub Actions runs on every push or PR to `develop` and `master`:

1. `dotnet restore`
2. `dotnet build --configuration Release`
3. `dotnet test --configuration Release`

Results are published via `dorny/test-reporter` as GitHub Checks.

---

## Key design decisions

See [`docs/adr/`](docs/adr/) for the full rationale behind each decision.

| ADR | Decision |
|---|---|
| [ADR-001](docs/adr/ADR-001-microsservicos-vs-monolito.md) | Microservices over modular monolith |
| [ADR-002](docs/adr/ADR-002-rabbitmq-para-ttl-de-reserva.md) | Background worker polling for ticket TTL expiry |
| [ADR-003](docs/adr/ADR-003-database-per-service.md) | Database-per-service |
| [ADR-004](docs/adr/ADR-004-autenticacao-jwt-com-refresh-token.md) | JWT + refresh token + service-to-service auth |
| [ADR-005](docs/adr/ADR-005-polly-resiliencia-http.md) | Polly for HTTP resilience between services |
| [ADR-006](docs/adr/ADR-006-application-service-em-vez-de-mediatr.md) | Application Services instead of MediatR |
| [ADR-007](docs/adr/ADR-007-ef-core-em-vez-de-dapper.md) | EF Core instead of Dapper |
