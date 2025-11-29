# HealthCore .NET – Laboratory Orders & Results Platform
<div align="center">
  <img src="thumbnail.png" width="350" />
</div>
A production-like **ASP.NET Core** application that simulates the core workflows of a medical laboratory:
creating lab orders, processing results asynchronously via **RabbitMQ**, and exposing them to clinical systems
through a secure REST API.

This project was built to showcase **real-world experience in healthcare systems** (LIS/RIS, HL7, DICOM) using
modern **ASP.NET Core** architecture, following the [ASP.NET Core Developer Roadmap](https://roadmap.sh/aspnet-core).

---

## 1. Problem Statement

Modern hospitals and diagnostic centers often have:

- Multiple systems (LIS, RIS, PACS, analyzers) that need to exchange data.
- Manual steps in lab order and result workflows.
- Difficulty tracking requests and results end-to-end.
- Limited observability in distributed environments.

This project simulates a **lab integration platform** that:

- Receives exam orders from clinical systems.
- Orchestrates the processing of those orders.
- Receives and validates results from analyzers and internal microservices.
- Exposes a clean API for consuming results.

---

## 2. High-Level Solution

Healthcore .NET is composed of:

- A **public Web API** (ASP.NET Core 8) for managing exam orders and results.
- A **background worker** that consumes messages from **RabbitMQ** and updates exam status.
- A **front-end dashboard** (Vue.js or React) to visualize pending and completed exams.
- **Observability** via OpenTelemetry (traces, metrics and logs).
- **Clean Architecture / DDD-inspired** organization for maintainability and testability.

---

## 3. Architecture Overview

### 3.1 Logical Components

- **LabConnect.Api**  
  ASP.NET Core 8 Web API exposing endpoints for:
  - Creating lab orders
  - Listing pending exams
  - Updating and retrieving lab results

- **LabConnect.Worker**  
  ASP.NET Core Background Service that:
  - Consumes messages from **RabbitMQ**
  - Simulates analyzer / LIS integrations
  - Applies **idempotency** rules to avoid duplicated processing

- **LabConnect.Domain**  
  - Core business entities (LabOrder, Exam, Result, Patient).
  - Domain services and value objects.

- **LabConnect.Infrastructure**
  - EF Core / Dapper database access (PostgreSQL or SQL Server).
  - Message broker integration (RabbitMQ).
  - Repository implementations.

- **LabConnect.WebApp** (optional but recommended)
  - Vue.js or React SPA for:
    - Worklist visualization (pending exams)
    - Filters and search
    - Status and turnaround time tracking

### 3.2 Technologies Used

- **Backend**
  - C# / .NET 8
  - ASP.NET Core Web API (Minimal APIs or Controllers)
  - Entity Framework Core (or Dapper)
  - RabbitMQ (message broker)
  - OpenTelemetry (tracing, metrics, logging)
  - xUnit + FluentAssertions + testcontainers for integration tests

- **Frontend**
  - Vue.js or React
  - TypeScript (optional but recommended)
  - Axios or Fetch for API calls

- **Infrastructure**
  - PostgreSQL or MySQL
  - Docker & Docker Compose
  - GitHub Actions (CI/CD)

---

## 4. ASP.NET Core Roadmap Alignment

This project intentionally covers several areas from the **ASP.NET Core Developer Roadmap**:

1. **General Development Skills**
   - Git, GitHub-based workflow
   - HTTP, REST, JSON, status codes

2. **C# / .NET**
   - .NET 8 SDK and CLI
   - Async/await, LINQ, records, pattern matching
   - Clean Code and SOLID principles

3. **ASP.NET Core Fundamentals**
   - Minimal APIs / Controllers
   - Middleware pipeline (logging, correlation ID, error handling)
   - Authentication & Authorization (JWT Bearer or cookie-based)
   - Configuration by environment (appsettings.Development/Production)
   - Background Services (Worker Service)

4. **Data Access**
   - EF Core DbContext, migrations, repositories
   - Transactions and optimistic concurrency
   - Basic performance considerations (indexes, no-tracking queries)

5. **Testing**
   - Unit tests (xUnit)
   - Integration tests with in-memory or containerized DB
   - Contract-style tests for APIs (e.g., using WebApplicationFactory)

6. **Microservices & Messaging**
   - RabbitMQ integration
   - Event-driven communication between API and Worker
   - Idempotent message processing

7. **Observability**
   - OpenTelemetry instrumentation
   - Export to Jaeger or OTLP compatible backend
   - Correlation IDs across API and Worker

8. **DevOps & CI/CD**
   - Dockerfile and docker-compose
   - GitHub Actions pipeline for build, test and Docker image publish

---

## 5. Features

- **Lab Order Management**
  - Create a lab order with patient, exams and priority.
  - List pending exams with filters by status, date or patient.

- **Asynchronous Result Processing**
  - API publishes “exam requested” messages to RabbitMQ.
  - Worker consumes and simulates result generation.
  - Exam status updated to *Processing* → *Completed*.

- **Idempotent Message Handling**
  - Each message has a unique ID.
  - Processed messages are tracked to prevent duplicates.

- **Turnaround Time Tracking**
  - Each exam records creation/completion timestamps.
  - API exposes metrics for average turnaround time per exam type.

- **Observability & Tracing**
  - Each request gets a correlation ID.
  - Traces propagate from API → RabbitMQ → Worker.
  - Export to Jaeger / OpenTelemetry Collector.

---

## 6. Domain Model (simplified)

- `LabOrder`
  - Id
  - PatientId
  - CreatedAt
  - Status (Pending, InProgress, Completed, Cancelled)
  - Exams: `ICollection<Exam>`

- `Exam`
  - Id
  - LabOrderId
  - Code (e.g. “GLUCOSE”, “PCR”)
  - Status
  - ResultId (optional)

- `Result`
  - Id
  - ExamId
  - Value
  - Unit
  - ReferenceRange
  - ReleasedAt

---

## 7. API Endpoints (examples)

```http
POST /api/lab-orders
GET  /api/lab-orders/{id}
GET  /api/lab-orders?status=Pending
GET  /api/exams/{id}
GET  /api/exams?status=Completed
