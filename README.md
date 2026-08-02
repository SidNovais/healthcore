<div align="center">
  <img src="thumbnail.png" width="350" alt="HealthCore logo" />

  <h1>HealthCore .NET — Laboratory Information System</h1>

  <p><em>A production-like ASP.NET Core modular monolith that simulates the end-to-end workflows of a medical laboratory (LIS/RIS).</em></p>

  <p>
    <img alt="CI" src="https://github.com/SidNovais/healthcore/actions/workflows/ci.yml/badge.svg" />
    <img alt=".NET" src="https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white" />
    <img alt="Angular" src="https://img.shields.io/badge/Angular-21-DD0031?logo=angular&logoColor=white" />
    <img alt="PostgreSQL" src="https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white" />
  </p>
</div>

---

> **Architecture credit.** The modular-monolith structure and the DDD building blocks (aggregates, domain events, outbox/inbox, CQRS, Autofac modules) are inspired by Kamil Grzybek's excellent
> [**ardalis/modular-monolith-with-ddd**](https://github.com/ardalis/modular-monolith-with-ddd). HealthCore adapts those patterns to a healthcare domain and a .NET 10 stack, and layers on event sourcing, an HL7/MLLP integration service, an Angular SPA, and a full observability stack.

---

## Table of Contents

- [Overview](#overview)
- [What's Implemented](#whats-implemented)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Testing](#testing)
- [Project Structure](#project-structure)
- [CI/CD](#cicd)
- [Acknowledgements](#acknowledgements)

---

## Overview

**HealthCore** is a reference implementation of a **Laboratory Information System (LIS)** built as a **modular monolith** with clean architecture and domain-driven design. It models the real lifecycle of laboratory work — from a physician requesting exams, through patient reception and specimen collection, to analyzer processing, result review, electronic signing, and report delivery.

It is intentionally *production-like* rather than a toy: strict build gates (`TreatWarningsAsErrors`, full analyzers), transactional messaging, event sourcing, real-time UI updates, and instrument connectivity over HL7. It exists to demonstrate how these concerns fit together in a maintainable, testable codebase.

**Key characteristics**

- **Six autonomous modules** communicating only through integration events — no cross-module database access.
- **Event-sourced aggregates** (Marten) with a transactional **outbox/inbox** and asynchronous **internal commands**.
- **CQRS** throughout — commands mutate state, queries read purpose-built read models.
- **Real-world integration** — a TCP/HL7 (MLLP) service for analyzer connectivity, plus an equipment simulator.
- **Angular 21 SPA** driven by an SDK auto-generated from the API's OpenAPI schema.
- **First-class observability** — OpenTelemetry traces & metrics exported to Jaeger, Prometheus, and Grafana.

---

## What's Implemented

### Business modules

Each module owns its schema, aggregates, application layer, and integration-event contracts, and exposes a single facade (`I{Module}Module`).

| Module | Responsibility |
|---|---|
| **TestOrders** | The exam-order lifecycle. `Order` / `OrderItem` aggregate with a full state machine and ~40 business-rule classes enforcing every transition. *(Reference module — copy this when adding new ones.)* |
| **SampleCollection** | Patient reception & waiting queue, calling patients, specimen barcoding, and recording sample collection. |
| **Analyzer** | Analyzer / instrument connectivity. Handles the barcode query → exam-list → result exchange over HL7; hosted by the TCP/MLLP service. |
| **LabAnalysis** | Result review, PDF report generation (QuestPDF), electronic signing, and report storage (HTML / S3). |
| **PatientManagement** | Patient registry and master data — register, update, and anonymize (GDPR-style). |
| **UserAccess** | Authentication, roles & authorization, and the audit log. JWT tokens with PBKDF2 password hashing. |

### The order lifecycle (worked example)

`TestOrders` models an exam item as a state machine, driven by commands and enforced by business rules:

```
Requested ──► Accepted ──► InProgress ──► PartiallyCompleted ──► Completed
     │            │
     └──► Rejected └──► OnHold                (Canceled is reachable as a side-state)
```

State advances through cross-module choreography: `SampleCollection` collecting a sample raises an integration event that moves the exam to **InProgress**; `LabAnalysis` completing a worklist item moves it to **Completed** — always via internal commands for transactional safety.

### API

An ASP.NET Core **Minimal API**, versioned under `/api/v1`, secured with JWT (delivered as an `HttpOnly` cookie), documented with Swagger, streaming real-time updates over **Server-Sent Events**, and protected by a configurable **rate limiter** (strict per-IP throttling on the anonymous auth surface, a per-user baseline elsewhere, and a concurrency cap on the SSE stream).

| Group | Purpose |
|---|---|
| `auth` (anonymous) | Login and current-session (`/auth/me`). |
| `orders` | Create and manage exam orders. |
| `samples`, `collection-requests` | Specimen collection workflow. |
| `analyzer-samples` | Analyzer sample data. |
| `worklist-items` | Result review, reporting, signing. |
| `patients` | Patient registry. |
| `users`, `audit-log` | User administration and auditing. |
| `events` (SSE) | `GET /api/v1/events/stream` — live push of integration events to the UI. |

Authorization policies: **`ITAdmin`** and **`PatientManagement`** (Receptionist or ITAdmin).

### Frontend

An **Angular 21** single-page app (Yarn workspaces monorepo) covering login, orders, patients, triage/reception, and the analysis worklist — with skeleton/spinner/progress loading standards, GSAP motion, and a11y-gated e2e tests. Its API client (`@hc-lis/api-client`) is generated from the API's OpenAPI schema, so the SPA stays in lockstep with the backend contract.

### Analyzer integration (HL7 / MLLP)

The **`HC.LIS.TcpMessage`** service is a standalone worker that listens on a raw TCP socket for **HL7 messages framed with MLLP** (optional TLS 1.3). The included **`HC.LIS.EquipmentSimulator`** plays the role of a bench analyzer, exercising the real exchange: `QBP^Q11` barcode query → `RSP^K11` exam list → `ORU` result → `ACK`.

### Observability

Every request and background job is traced. OpenTelemetry exports traces and metrics through an OTel Collector to **Jaeger** (traces) and **Prometheus** + **Grafana** (metrics/dashboards). Logs are structured (Serilog) and correlated by trace ID.

---

## Architecture

HealthCore is a **modular monolith**: modules are isolated like microservices (own schema, own events, no shared tables) but deployed as one process. Each module follows **Clean Architecture** layering — `Domain → Application → Infrastructure` — enforced automatically by `NetArchTest` architecture tests.

```mermaid
flowchart TB
    SPA["Angular 21 SPA<br/>(:4200)"]
    SIM["Equipment Simulator<br/>(HL7 analyzer)"]

    subgraph API["HC.LIS.API — Minimal API (:5000)"]
        MODS["Modules:<br/>TestOrders · SampleCollection · Analyzer<br/>LabAnalysis · PatientManagement · UserAccess"]
        SSE["SSE /events/stream"]
    end

    TCP["HC.LIS.TcpMessage<br/>TCP/HL7 · MLLP (:8890)<br/>hosts Analyzer module"]

    subgraph DATA["Data & Messaging"]
        PG[("PostgreSQL<br/>Marten event store<br/>+ read models")]
        BUS{{"Event bus<br/>RabbitMQ or in-memory"}}
    end

    subgraph OBS["Observability"]
        OTEL["OTel Collector"]
        JAEGER["Jaeger (:16686)"]
        PROM["Prometheus (:9090)"]
        GRAF["Grafana (:3000)"]
    end

    SPA -->|"REST /api/v1"| API
    SSE -->|"real-time push"| SPA
    SIM <-->|"HL7 over MLLP"| TCP

    MODS -->|"append events / read"| PG
    TCP -->|"append events / read"| PG
    MODS -->|"outbox → publish / inbox → consume"| BUS
    BUS -->|"integration events"| MODS

    API --> OTEL
    TCP --> OTEL
    OTEL --> JAEGER
    OTEL --> PROM
    PROM --> GRAF
```

### Patterns in play

- **Event Sourcing** — aggregate state is rebuilt by replaying domain events (Marten).
- **CQRS** — separate command and query paths, each with paired handlers.
- **Transactional Outbox + Inbox** — domain events persist atomically with the aggregate; Quartz.NET jobs relay them to the bus and process inbound events.
- **Internal Commands** — inbound integration events schedule internal commands (persisted, executed asynchronously) instead of running work inline.
- **Business rules as objects** — each invariant is its own `IBusinessRule` class.
- **MediatR pipeline decorators** — validation, logging, and unit-of-work as cross-cutting concerns.
- **Autofac modules** — DI composition split per concern (data access, outbox, Quartz, event bus, …).

> The event bus is **optional**. With `EventBus__Type=rabbitmq` (default) integration events cross the process boundary via RabbitMQ; with `EventBus__Type=memory` they stay in-process — handy for a zero-broker local run.

For the full architecture reference — layer rules, provider-interface pattern, naming conventions, and the "add a feature" playbook — see [`CLAUDE.md`](CLAUDE.md).

---

## Tech Stack

| Area | Technologies |
|---|---|
| **Backend** | .NET 10, C# 13, ASP.NET Core Minimal APIs, Autofac, MediatR, FluentValidation |
| **Data & messaging** | PostgreSQL, Marten (event store), Dapper (read models), FluentMigrator, Quartz.NET, RabbitMQ *(optional)*, DistributedLock.Postgres |
| **Frontend** | Angular 21, TypeScript, Yarn workspaces, GSAP, OpenAPI-generated client (`@hey-api/openapi-ts`) |
| **Observability** | OpenTelemetry, OTel Collector, Jaeger, Prometheus, Grafana, Serilog |
| **Reporting** | QuestPDF (report PDFs), AWS SDK for S3 (report storage) |
| **Testing** | xUnit, FluentAssertions, NSubstitute, NetArchTest.Rules, Vitest, Playwright (+ axe-core) |
| **DevOps** | Docker & Docker Compose, GitHub Actions, GHCR |

---

## Getting Started

### Prerequisites

- **.NET SDK 10.0.100+** (pinned in [`global.json`](global.json))
- **Docker** & Docker Compose (for PostgreSQL and the supporting services)
- **Node.js 20+** and **Yarn (classic, v1)** — for the frontend

> There is **no `.sln` file** — projects are linked by convention via `Directory.Build.targets`. Build and run individual `.csproj` files; CI generates a full solution on the fly.

### Option A — One-click with VS Code (recommended)

The repo ships `.vscode/launch.json` + `tasks.json` with everything pre-wired (including dev JWT values), so you can go from clone to running with a single launch:

1. **Prepare DEV ENVIROMENT (Docker)** — starts the compose stack (Postgres, RabbitMQ, observability) and runs database migrations.
2. **Full Stack (API + SPA)** *(compound)* — launches the API (in-memory bus) and the Angular dev server together and opens Chrome at `http://localhost:4200`.

Other ready-made launch profiles: **HC.LIS.API**, **HC.LIS.API (Memory Bus)**, **HC.LIS.TcpMessage**, **HC.LIS.SPA**, and **Healthcore.Database**.

### Option B — Manual / CLI

**1. Start infrastructure**

```bash
docker-compose -f development-compose.yaml up -d
```

Brings up PostgreSQL, RabbitMQ, the OTel Collector, Jaeger, Prometheus, and Grafana. (The API is **not** in compose — you run it on the host.)

**2. Apply database migrations** (creates the relational schema, Marten event-store schema, and seed users)

```bash
export ASPNETCORE_HCLIS_DATABASE_CONNECTION_STRING="Host=localhost;Port=5432;Database=Healthcore.Dev;Username=dev;Password=dev"
dotnet run --project src/HC.LIS/HC.LIS.Database
```

**3. Run the API** (requires the DB connection string plus JWT settings)

```bash
export ASPNETCORE_HCLIS_JWT_ISSUER="hclis"
export ASPNETCORE_HCLIS_JWT_AUDIENCE="hclis"
export ASPNETCORE_HCLIS_JWT_SECRET_KEY="change-me-in-production-min-32-chars!!"
export ASPNETCORE_HCLIS_EventBus__Type="memory"   # or "rabbitmq" (default)
dotnet run --project src/HC.LIS/HC.LIS.API
```

The API listens on `http://localhost:5000`; Swagger UI is at `http://localhost:5000/swagger`.

**4. Run the frontend**

```bash
cd src/HC.LIS.Frontend
yarn install

# Generate the typed API client from the running API's OpenAPI schema, then build it:
cd packages/hc-lis-api-client
yarn generate      # reads http://localhost:5000/swagger/v1/swagger.json (override with SWAGGER_URL)
yarn build

# Start the SPA
cd ../hc-lis-spa
yarn start         # http://localhost:4200 (proxies /api → http://localhost:5000)
```

**5. (Optional) Analyzer integration**

```bash
# Terminal 1 — the HL7/MLLP listener
dotnet run --project src/HC.LIS/HC.LIS.TcpMessage/TcpMessage      # TCP :8890

# Terminal 2 — simulate a bench analyzer talking to it
dotnet run --project src/HC.LIS/HC.LIS.EquipmentSimulator
```

### Seed users

All seeded accounts share the dev password **`Admin1234!`**:

| Email | Role |
|---|---|
| `root@hclis.local` | ITAdmin |
| `receptionist@hclis.local` | Receptionist |
| `labtech@hclis.local` | LabTechnician |
| `physician@hclis.local` | Physician |

### Where things live

| Service | URL |
|---|---|
| SPA | http://localhost:4200 |
| API / Swagger | http://localhost:5000 / http://localhost:5000/swagger |
| Jaeger (traces) | http://localhost:16686 |
| Prometheus (metrics) | http://localhost:9090 |
| Grafana (dashboards) | http://localhost:3000 |
| RabbitMQ management | http://localhost:15672 (`dev` / `dev`) |

---

## Configuration

Configuration is read from environment variables prefixed **`ASPNETCORE_HCLIS_`**; nested keys use a double underscore (`__` → `:`). Nothing sensitive lives in `appsettings.json`.

| Variable | Used by | Required | Default / notes |
|---|---|:---:|---|
| `ASPNETCORE_HCLIS_DATABASE_CONNECTION_STRING` | API, TcpMessage, migrations | ✅ | Npgsql/PostgreSQL connection string |
| `ASPNETCORE_HCLIS_JWT_ISSUER` | API | ✅ | JWT issuer |
| `ASPNETCORE_HCLIS_JWT_AUDIENCE` | API | ✅ | JWT audience |
| `ASPNETCORE_HCLIS_JWT_SECRET_KEY` | API | ✅ | HMAC signing key (min 32 chars) |
| `ASPNETCORE_HCLIS_JWT_COOKIE_NAME` | API | — | Cookie the JWT is read from (default `ACCESS_TOKEN`) |
| `ASPNETCORE_HCLIS_RateLimit__Enabled` | API | — | Master rate-limit switch (default `true`); set `false` for e2e / integration-test runs |
| `ASPNETCORE_HCLIS_RateLimit__Global__PermitLimit` / `__Global__WindowSeconds` | API | — | Per-user (else per-IP) baseline for all `/api/*` (default `100` / `60`s) |
| `ASPNETCORE_HCLIS_RateLimit__Auth__PermitLimit` / `__Auth__WindowSeconds` | API | — | Strict per-IP limit on anonymous auth endpoints — login, activation (default `10` / `60`s) |
| `ASPNETCORE_HCLIS_RateLimit__Stream__PermitLimit` | API | — | Max concurrent SSE `/events/stream` connections per user (default `5`) |
| `ASPNETCORE_HCLIS_KNOWN_PROXIES` | API | — | Comma-separated trusted proxy IPs; enables `X-Forwarded-For` so per-IP limits see the real client (default: loopback only) |
| `ASPNETCORE_HCLIS_EventBus__Type` | API | — | `rabbitmq` (default) or `memory` (in-process) |
| `ASPNETCORE_HCLIS_EventBus__ConnectionString` | API | — | AMQP URI; default `amqp://guest:guest@localhost:5672/` (compose uses `amqp://dev:dev@localhost:5672/`) |
| `ASPNETCORE_HCLIS_Tcp__Port` | TcpMessage | — | HL7/MLLP listener port (default `8890`) |
| `ASPNETCORE_HCLIS_Tcp__UseTls` | TcpMessage | — | Enable TLS 1.3 (default `false`); pair with `Tcp__TlsCertificatePath` / `Tcp__TlsCertificatePassword` |
| `ASPNETCORE_HCLIS_Tcp__EnableMllpChecksum` / `Tcp__EnableHl7Checksum` | TcpMessage | — | Frame/message checksums (default `false`) |
| `ASPNETCORE_HCLIS_ROOT_PASSWORD_HASH` | migrations | — | Override the seeded root user's password hash for production |
| `ASPNETCORE_HCLIS_S3_BUCKET_NAME` / `S3_REGION` | LabAnalysis | — | Report storage (region default `us-east-1`) |
| `ASPNETCORE_HCLIS_IntegrationTests_ConnectionString` | tests only | — | Postgres connection string for integration tests |
| `API_TITLE` / `API_DESCRIPTION` | API | — | Swagger document metadata |

---

## Testing

**TDD is required** — the failing test comes first, and `test:` commits precede `feat:` commits (see [`CLAUDE.md`](CLAUDE.md)).

```bash
# Unit tests (per module)
dotnet test src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/UnitTests/HC.LIS.Modules.TestOrders.UnitTests.csproj

# Architecture tests (enforce Domain → Application → Infrastructure layering)
dotnet test src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/ArchTests/HC.LIS.Modules.TestOrders.ArchTests.csproj

# Integration tests — need Postgres + a connection string
export ASPNETCORE_HCLIS_IntegrationTests_ConnectionString="Host=localhost;Port=5432;Database=Healthcore.Dev;Username=dev;Password=dev"
dotnet test src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/IntegrationTests/HC.LIS.Modules.TestOrders.IntegrationTests.csproj

# A single test by name
dotnet test --filter "FullyQualifiedName~CreateOrderIsSuccessful" \
  src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/UnitTests/HC.LIS.Modules.TestOrders.UnitTests.csproj
```

**Frontend**

```bash
cd src/HC.LIS.Frontend/packages/hc-lis-spa
yarn test          # unit tests (Vitest)
yarn e2e           # end-to-end tests (Playwright) — requires the SPA + API + DB running
```

---

## Project Structure

```
src/
├── HC.Core/                          # Shared kernel — no business logic
│   ├── HC.Core/Domain/               # Entity, ValueObject, AggregateRoot, IBusinessRule, SystemClock
│   ├── HC.Core/Application/          # DomainEventsDispatcher, ProjectorBase, IExecutionContextAccessor
│   └── HC.Core/Infrastructure/       # IEventsBus, IOutbox, InboxMessage, real-time hub
│
├── HC.LIS/
│   ├── HC.LIS.API/                   # Public Minimal API host (auth, Swagger, SSE, OTel)
│   ├── HC.LIS.TcpMessage/            # TCP/HL7 (MLLP) analyzer listener
│   ├── HC.LIS.EquipmentSimulator/    # HL7 analyzer simulator (dev/test tool)
│   ├── HC.LIS.Database/              # FluentMigrator runner + Marten schema applier
│   ├── HC.LIS.Modules/               # TestOrders · SampleCollection · Analyzer
│   │                                 # LabAnalysis · PatientManagement · UserAccess
│   └── HC.LIS.Tests/                 # Global architecture + integration-event tests
│
└── HC.LIS.Frontend/                  # Angular 21 SPA (Yarn workspaces monorepo)
    └── packages/
        ├── hc-lis-spa/               # The Angular application
        └── hc-lis-api-client/        # OpenAPI-generated TypeScript SDK
```

---

## CI/CD

GitHub Actions ([`.github/workflows/ci.yml`](.github/workflows/ci.yml)) runs on every push and PR to `main`:

1. **Build & Unit Tests** — strict Release build (warnings fail) + all unit-test projects with coverage.
2. **Integration Tests** — against a `postgres:16` service container, with migrations applied first and per-test hang guards.
3. **Publish Docker Images** — on push, builds and pushes `hc-lis-api` and `hc-lis-tcpmessage` images to **GitHub Container Registry (GHCR)**, tagged `latest` (default branch), short-SHA, and semver (on `v*` tags).

---

## Acknowledgements

- Architecture and DDD patterns inspired by [**ardalis/modular-monolith-with-ddd**](https://github.com/ardalis/modular-monolith-with-ddd) by Kamil Grzybek.
- Built as a portfolio-grade reference implementation for healthcare systems engineering with modern ASP.NET Core.
