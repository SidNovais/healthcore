# Doctor Worklist — Patient Name, DOB & Gender Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Display patient full name, date of birth, and gender in the LabAnalysis physician worklist (list and detail views) instead of the patient UUID.

**Architecture:** LabAnalysis subscribes to `PatientRegistered/Updated/Anonymized` integration events from PatientManagement and maintains a local `lab_analysis."PatientSnapshotDetails"` read-model table — the same pattern already used by TestOrders. Both worklist query handlers are updated to LEFT JOIN this local snapshot, and the Angular templates are updated to render the new fields.

**Tech Stack:** C# 13 / .NET 10, FluentMigrator, Dapper, MediatR, Autofac, NSubstitute, xUnit, FluentAssertions, Angular 17+

---

## File Map

### New files
| File | Responsibility |
|---|---|
| `src/HC.LIS/HC.LIS.Database/Migrations/LabAnalysis/20260610120000_LabAnalysisModule_AddTablePatientSnapshotDetails.cs` | FluentMigrator migration creating the table |
| `…/LabAnalysis/Application/Patients/IPatientSnapshotRepository.cs` | Repository interface (Store / Update / Anonymize) |
| `…/Application/Patients/StorePatientSnapshot/StorePatientSnapshotByPatientIdCommand.cs` | Internal command |
| `…/Application/Patients/StorePatientSnapshot/StorePatientSnapshotByPatientIdCommandHandler.cs` | Calls `IPatientSnapshotRepository.StoreAsync()` |
| `…/Application/Patients/StorePatientSnapshot/PatientRegisteredIntegrationEventNotificationHandler.cs` | Enqueues store command |
| `…/Application/Patients/UpdatePatientSnapshot/UpdatePatientSnapshotByPatientIdCommand.cs` | Internal command |
| `…/Application/Patients/UpdatePatientSnapshot/UpdatePatientSnapshotByPatientIdCommandHandler.cs` | Calls `IPatientSnapshotRepository.UpdateAsync()` |
| `…/Application/Patients/UpdatePatientSnapshot/PatientUpdatedIntegrationEventNotificationHandler.cs` | Enqueues update command |
| `…/Application/Patients/AnonymizePatientSnapshot/AnonymizePatientSnapshotByPatientIdCommand.cs` | Internal command |
| `…/Application/Patients/AnonymizePatientSnapshot/AnonymizePatientSnapshotByPatientIdCommandHandler.cs` | Calls `IPatientSnapshotRepository.AnonymizeAsync()` |
| `…/Application/Patients/AnonymizePatientSnapshot/PatientAnonymizedIntegrationEventNotificationHandler.cs` | Enqueues anonymize command |
| `…/Infrastructure/Patients/PatientSnapshotRepository.cs` | Dapper implementation for `lab_analysis."PatientSnapshotDetails"` |
| `…/Tests/UnitTests/Patients/StorePatientSnapshotCommandHandlerTests.cs` | Unit tests for store command handler + integration event handler |
| `…/Tests/UnitTests/Patients/UpdatePatientSnapshotCommandHandlerTests.cs` | Unit tests for update command handler + integration event handler |
| `…/Tests/UnitTests/Patients/AnonymizePatientSnapshotCommandHandlerTests.cs` | Unit tests for anonymize command handler + integration event handler |
| `…/Tests/IntegrationTests/WorklistItems/WorklistItemPatientInfoTests.cs` | Integration tests asserting patient fields in list + detail queries |

### Modified files
| File | Change |
|---|---|
| `…/Application/HC.LIS.Modules.LabAnalysis.Application.csproj` | Add `<ProjectReference>` to `PatientManagement.IntegrationEvents` |
| `…/Infrastructure/Configurations/DataAccess/DataAccessModule.cs` | Register `PatientSnapshotRepository` as `IPatientSnapshotRepository` |
| `…/Infrastructure/Configurations/LabAnalysisStartup.cs` | Add 3 commands to `internalCommandsMap` BiMap |
| `…/Application/WorklistItems/GetWorklistItemList/WorklistItemSummaryDto.cs` | Add `PatientName`, `PatientDateOfBirth`, `PatientGender` |
| `…/Application/WorklistItems/GetWorklistItemList/GetWorklistItemListQueryHandler.cs` | Add LEFT JOIN with patient snapshot |
| `…/Application/WorklistItems/GetWorklistItemDetails/WorklistItemDetailsDto.cs` | Add same 3 fields |
| `…/Application/WorklistItems/GetWorklistItemDetails/GetWorklistItemDetailsQueryHandler.cs` | Add LEFT JOIN to first query |
| `…/Tests/IntegrationTests/TestBase.cs` | Add `DELETE FROM "lab_analysis"."PatientSnapshotDetails"` to `ClearDatabase` |
| `src/HC.LIS.Frontend/…/core/domain/worklist-item-summary.ts` | Add 3 nullable fields |
| `src/HC.LIS.Frontend/…/core/domain/worklist-item-details.ts` | Add 3 nullable fields |
| `src/HC.LIS.Frontend/…/features/worklist/worklist.component.ts` | Replace `Patient ID` column with `Patient` showing name |
| `src/HC.LIS.Frontend/…/features/worklist/worklist-item-detail.component.ts` | Replace patient GUID paragraph with name/DOB/gender |
| `src/HC.LIS.Frontend/…/e2e/worklist.spec.ts` | Assert patient name in worklist row |

> **Path prefix for all C# paths:** `src/HC.LIS/HC.LIS.Modules/LabAnalysis/`

---

## Task 1: DB Migration — Create `PatientSnapshotDetails` Table

**Files:**
- Create: `src/HC.LIS/HC.LIS.Database/Migrations/LabAnalysis/20260610120000_LabAnalysisModule_AddTablePatientSnapshotDetails.cs`

- [ ] **Step 1: Create the migration file**

```csharp
using FluentMigrator;

namespace HC.LIS.Database.LabAnalysis;

[Migration(20260610120000)]
public class LabAnalysisModuleAddTablePatientSnapshotDetails : Migration
{
    public override void Up()
    {
        Create.Table("PatientSnapshotDetails").InSchema("lab_analysis")
            .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("FullName").AsString(255).NotNullable()
            .WithColumn("DateOfBirth").AsCustom("TIMESTAMPTZ").NotNullable()
            .WithColumn("Gender").AsString(50).Nullable()
            .WithColumn("MothersFullName").AsString(255).Nullable()
            .WithColumn("DocumentId").AsString(100).Nullable()
            .WithColumn("Phone").AsString(50).Nullable()
            .WithColumn("Email").AsString(255).Nullable()
            .WithColumn("Status").AsString(50).NotNullable()
            .WithColumn("RegisteredAt").AsCustom("TIMESTAMPTZ").NotNullable()
            .WithColumn("AnonymizedAt").AsCustom("TIMESTAMPTZ").Nullable();
    }

    public override void Down()
    {
        Delete.Table("PatientSnapshotDetails").InSchema("lab_analysis");
    }
}
```

- [ ] **Step 2: Run the migration** (DB must be running)

```bash
dotnet run --project src/HC.LIS/HC.LIS.Database/HC.LIS.Database.csproj
```

Expected: Migration `20260610120000` applied, no errors.

- [ ] **Step 3: Commit**

```bash
git add src/HC.LIS/HC.LIS.Database/Migrations/LabAnalysis/20260610120000_LabAnalysisModule_AddTablePatientSnapshotDetails.cs
git commit -m "feat(lab-analysis): add PatientSnapshotDetails migration"
```

---

## Task 2: Application Project Reference + Repository Interface

**Files:**
- Modify: `src/HC.LIS/HC.LIS.Modules/LabAnalysis/Application/HC.LIS.Modules.LabAnalysis.Application.csproj`
- Create: `src/HC.LIS/HC.LIS.Modules/LabAnalysis/Application/Patients/IPatientSnapshotRepository.cs`

- [ ] **Step 1: Add `PatientManagement.IntegrationEvents` project reference**

In `HC.LIS.Modules.LabAnalysis.Application.csproj`, add inside the existing `<ItemGroup>`:

```xml
<ItemGroup>
  <ProjectReference Include="..\..\SampleCollection\IntegrationEvents\HC.LIS.Modules.SampleCollection.IntegrationEvents.csproj" />
  <ProjectReference Include="..\..\PatientManagement\IntegrationEvents\HC.LIS.Modules.PatientManagement.IntegrationEvents.csproj" />
</ItemGroup>
```

- [ ] **Step 2: Create the repository interface**

```csharp
namespace HC.LIS.Modules.LabAnalysis.Application.Patients;

public interface IPatientSnapshotRepository
{
    Task StoreAsync(
        Guid patientId,
        string fullName,
        DateTime dateOfBirth,
        string? gender,
        string? mothersFullName,
        string? documentId,
        string? phone,
        string? email,
        DateTime registeredAt);

    Task UpdateAsync(
        Guid patientId,
        string fullName,
        DateTime dateOfBirth,
        string? gender,
        string? mothersFullName,
        string? documentId,
        string? phone,
        string? email);

    Task AnonymizeAsync(Guid patientId, DateTime anonymizedAt);
}
```

- [ ] **Step 3: Verify build**

```bash
dotnet build src/HC.LIS/HC.LIS.Modules/LabAnalysis/Application/HC.LIS.Modules.LabAnalysis.Application.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Commit**

```bash
git add src/HC.LIS/HC.LIS.Modules/LabAnalysis/Application/HC.LIS.Modules.LabAnalysis.Application.csproj
git add src/HC.LIS/HC.LIS.Modules/LabAnalysis/Application/Patients/IPatientSnapshotRepository.cs
git commit -m "feat(lab-analysis): add IPatientSnapshotRepository interface + PatientManagement IntegrationEvents reference"
```

---

## Task 3: Store Patient Snapshot (Command + Handler + Integration Event Handler)

**Files:**
- Create: `…/Application/Patients/StorePatientSnapshot/StorePatientSnapshotByPatientIdCommand.cs`
- Create: `…/Application/Patients/StorePatientSnapshot/StorePatientSnapshotByPatientIdCommandHandler.cs`
- Create: `…/Application/Patients/StorePatientSnapshot/PatientRegisteredIntegrationEventNotificationHandler.cs`
- Create: `…/Tests/UnitTests/Patients/StorePatientSnapshotCommandHandlerTests.cs`

- [ ] **Step 1: Write the failing unit tests**

```csharp
using FluentAssertions;
using HC.LIS.Modules.LabAnalysis.Application.Patients;
using HC.LIS.Modules.LabAnalysis.Application.Patients.StorePatientSnapshot;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.PatientManagement.IntegrationEvents;
using NSubstitute;

namespace HC.LIS.Modules.LabAnalysis.UnitTests.Patients;

public class StorePatientSnapshotCommandHandlerTests
{
    private static readonly Guid PatientId = Guid.Parse("019b664c-52a4-7f37-a794-000000000001");
    private static readonly DateTime DateOfBirth = new(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime RegisteredAt = new(2026, 6, 10, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Handler_StoresSnapshotWithAllFields()
    {
        IPatientSnapshotRepository repo = Substitute.For<IPatientSnapshotRepository>();
        var handler = new StorePatientSnapshotByPatientIdCommandHandler(repo);
        var command = new StorePatientSnapshotByPatientIdCommand(
            Guid.CreateVersion7(),
            PatientId,
            "John Doe",
            DateOfBirth,
            "Male",
            "Jane Doe",
            "DOC123",
            "+1-555-0100",
            "john@example.com",
            RegisteredAt
        );

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        await repo.Received(1).StoreAsync(
            PatientId,
            "John Doe",
            DateOfBirth,
            "Male",
            "Jane Doe",
            "DOC123",
            "+1-555-0100",
            "john@example.com",
            RegisteredAt
        ).ConfigureAwait(true);
    }

    [Fact]
    public async Task IntegrationEventHandler_EnqueuesStoreCommand()
    {
        ICommandsScheduler scheduler = Substitute.For<ICommandsScheduler>();
        var handler = new PatientRegisteredIntegrationEventNotificationHandler(scheduler);
        var notification = new PatientRegisteredIntegrationEvent(
            Guid.CreateVersion7(),
            RegisteredAt,
            PatientId,
            "John Doe",
            DateOfBirth,
            "Male",
            null,
            null,
            null,
            null,
            RegisteredAt
        );

        await handler.Handle(notification, CancellationToken.None).ConfigureAwait(true);

        await scheduler.Received(1)
            .EnqueueAsync(Arg.Is<StorePatientSnapshotByPatientIdCommand>(c =>
                c.PatientId == PatientId &&
                c.FullName == "John Doe" &&
                c.DateOfBirth == DateOfBirth &&
                c.Gender == "Male" &&
                c.RegisteredAt == RegisteredAt
            )).ConfigureAwait(true);
    }
}
```

- [ ] **Step 2: Run tests to confirm they fail**

```bash
dotnet test src/HC.LIS/HC.LIS.Modules/LabAnalysis/Tests/UnitTests/HC.LIS.Modules.LabAnalysis.UnitTests.csproj --filter "FullyQualifiedName~StorePatientSnapshot"
```

Expected: Compiler errors — types don't exist yet.

- [ ] **Step 3: Create the internal command**

```csharp
using System.Text.Json.Serialization;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Patients.StorePatientSnapshot;

[method: JsonConstructor]
public class StorePatientSnapshotByPatientIdCommand(
    Guid id,
    Guid patientId,
    string fullName,
    DateTime dateOfBirth,
    string? gender,
    string? mothersFullName,
    string? documentId,
    string? phone,
    string? email,
    DateTime registeredAt
) : InternalCommandBase(id)
{
    public Guid PatientId { get; } = patientId;
    public string FullName { get; } = fullName;
    public DateTime DateOfBirth { get; } = dateOfBirth;
    public string? Gender { get; } = gender;
    public string? MothersFullName { get; } = mothersFullName;
    public string? DocumentId { get; } = documentId;
    public string? Phone { get; } = phone;
    public string? Email { get; } = email;
    public DateTime RegisteredAt { get; } = registeredAt;
}
```

- [ ] **Step 4: Create the command handler**

```csharp
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Patients.StorePatientSnapshot;

internal class StorePatientSnapshotByPatientIdCommandHandler(
    IPatientSnapshotRepository patientSnapshotRepository
) : ICommandHandler<StorePatientSnapshotByPatientIdCommand>
{
    private readonly IPatientSnapshotRepository _patientSnapshotRepository = patientSnapshotRepository;

    public async Task Handle(
        StorePatientSnapshotByPatientIdCommand command,
        CancellationToken cancellationToken)
    {
        await _patientSnapshotRepository.StoreAsync(
            command.PatientId,
            command.FullName,
            command.DateOfBirth,
            command.Gender,
            command.MothersFullName,
            command.DocumentId,
            command.Phone,
            command.Email,
            command.RegisteredAt
        ).ConfigureAwait(false);
    }
}
```

- [ ] **Step 5: Create the integration event handler**

```csharp
using MediatR;
using HC.LIS.Modules.PatientManagement.IntegrationEvents;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Patients.StorePatientSnapshot;

public class PatientRegisteredIntegrationEventNotificationHandler(ICommandsScheduler commandsScheduler)
    : INotificationHandler<PatientRegisteredIntegrationEvent>
{
    private readonly ICommandsScheduler _commandsScheduler = commandsScheduler;

    public async Task Handle(
        PatientRegisteredIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        await _commandsScheduler.EnqueueAsync(new StorePatientSnapshotByPatientIdCommand(
            Guid.CreateVersion7(),
            notification.PatientId,
            notification.FullName,
            notification.DateOfBirth,
            notification.Gender,
            notification.MothersFullName,
            notification.DocumentId,
            notification.Phone,
            notification.Email,
            notification.RegisteredAt
        )).ConfigureAwait(false);
    }
}
```

- [ ] **Step 6: Run tests to confirm they pass**

```bash
dotnet test src/HC.LIS/HC.LIS.Modules/LabAnalysis/Tests/UnitTests/HC.LIS.Modules.LabAnalysis.UnitTests.csproj --filter "FullyQualifiedName~StorePatientSnapshot"
```

Expected: 2 tests pass.

- [ ] **Step 7: Commit (test first, then feat)**

```bash
git add "src/HC.LIS/HC.LIS.Modules/LabAnalysis/Tests/UnitTests/Patients/StorePatientSnapshotCommandHandlerTests.cs"
git commit -m "test(lab-analysis): store patient snapshot command handler and integration event handler"

git add "src/HC.LIS/HC.LIS.Modules/LabAnalysis/Application/Patients/StorePatientSnapshot/"
git commit -m "feat(lab-analysis): store patient snapshot on PatientRegistered event"
```

---

## Task 4: Update Patient Snapshot (Command + Handler + Integration Event Handler)

**Files:**
- Create: `…/Application/Patients/UpdatePatientSnapshot/UpdatePatientSnapshotByPatientIdCommand.cs`
- Create: `…/Application/Patients/UpdatePatientSnapshot/UpdatePatientSnapshotByPatientIdCommandHandler.cs`
- Create: `…/Application/Patients/UpdatePatientSnapshot/PatientUpdatedIntegrationEventNotificationHandler.cs`
- Create: `…/Tests/UnitTests/Patients/UpdatePatientSnapshotCommandHandlerTests.cs`

- [ ] **Step 1: Write the failing unit tests**

```csharp
using FluentAssertions;
using HC.LIS.Modules.LabAnalysis.Application.Patients;
using HC.LIS.Modules.LabAnalysis.Application.Patients.UpdatePatientSnapshot;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.PatientManagement.IntegrationEvents;
using NSubstitute;

namespace HC.LIS.Modules.LabAnalysis.UnitTests.Patients;

public class UpdatePatientSnapshotCommandHandlerTests
{
    private static readonly Guid PatientId = Guid.Parse("019b664c-52a4-7f37-a794-000000000002");
    private static readonly DateTime DateOfBirth = new(1985, 3, 20, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime OccurredAt = new(2026, 6, 10, 13, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Handler_UpdatesSnapshotWithAllFields()
    {
        IPatientSnapshotRepository repo = Substitute.For<IPatientSnapshotRepository>();
        var handler = new UpdatePatientSnapshotByPatientIdCommandHandler(repo);
        var command = new UpdatePatientSnapshotByPatientIdCommand(
            Guid.CreateVersion7(),
            PatientId,
            "Jane Smith",
            DateOfBirth,
            "Female",
            null,
            "DOC456",
            null,
            "jane@example.com"
        );

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        await repo.Received(1).UpdateAsync(
            PatientId,
            "Jane Smith",
            DateOfBirth,
            "Female",
            null,
            "DOC456",
            null,
            "jane@example.com"
        ).ConfigureAwait(true);
    }

    [Fact]
    public async Task IntegrationEventHandler_EnqueuesUpdateCommand()
    {
        ICommandsScheduler scheduler = Substitute.For<ICommandsScheduler>();
        var handler = new PatientUpdatedIntegrationEventNotificationHandler(scheduler);
        var notification = new PatientUpdatedIntegrationEvent(
            Guid.CreateVersion7(),
            OccurredAt,
            PatientId,
            "Jane Smith",
            DateOfBirth,
            "Female",
            null,
            "DOC456",
            null,
            "jane@example.com"
        );

        await handler.Handle(notification, CancellationToken.None).ConfigureAwait(true);

        await scheduler.Received(1)
            .EnqueueAsync(Arg.Is<UpdatePatientSnapshotByPatientIdCommand>(c =>
                c.PatientId == PatientId &&
                c.FullName == "Jane Smith"
            )).ConfigureAwait(true);
    }
}
```

- [ ] **Step 2: Run tests to confirm they fail**

```bash
dotnet test src/HC.LIS/HC.LIS.Modules/LabAnalysis/Tests/UnitTests/HC.LIS.Modules.LabAnalysis.UnitTests.csproj --filter "FullyQualifiedName~UpdatePatientSnapshot"
```

Expected: Compiler errors.

- [ ] **Step 3: Create the internal command**

```csharp
using System.Text.Json.Serialization;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Patients.UpdatePatientSnapshot;

[method: JsonConstructor]
public class UpdatePatientSnapshotByPatientIdCommand(
    Guid id,
    Guid patientId,
    string fullName,
    DateTime dateOfBirth,
    string? gender,
    string? mothersFullName,
    string? documentId,
    string? phone,
    string? email
) : InternalCommandBase(id)
{
    public Guid PatientId { get; } = patientId;
    public string FullName { get; } = fullName;
    public DateTime DateOfBirth { get; } = dateOfBirth;
    public string? Gender { get; } = gender;
    public string? MothersFullName { get; } = mothersFullName;
    public string? DocumentId { get; } = documentId;
    public string? Phone { get; } = phone;
    public string? Email { get; } = email;
}
```

- [ ] **Step 4: Create the command handler**

```csharp
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Patients.UpdatePatientSnapshot;

internal class UpdatePatientSnapshotByPatientIdCommandHandler(
    IPatientSnapshotRepository patientSnapshotRepository
) : ICommandHandler<UpdatePatientSnapshotByPatientIdCommand>
{
    private readonly IPatientSnapshotRepository _patientSnapshotRepository = patientSnapshotRepository;

    public async Task Handle(
        UpdatePatientSnapshotByPatientIdCommand command,
        CancellationToken cancellationToken)
    {
        await _patientSnapshotRepository.UpdateAsync(
            command.PatientId,
            command.FullName,
            command.DateOfBirth,
            command.Gender,
            command.MothersFullName,
            command.DocumentId,
            command.Phone,
            command.Email
        ).ConfigureAwait(false);
    }
}
```

- [ ] **Step 5: Create the integration event handler**

```csharp
using MediatR;
using HC.LIS.Modules.PatientManagement.IntegrationEvents;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Patients.UpdatePatientSnapshot;

public class PatientUpdatedIntegrationEventNotificationHandler(ICommandsScheduler commandsScheduler)
    : INotificationHandler<PatientUpdatedIntegrationEvent>
{
    private readonly ICommandsScheduler _commandsScheduler = commandsScheduler;

    public async Task Handle(
        PatientUpdatedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        await _commandsScheduler.EnqueueAsync(new UpdatePatientSnapshotByPatientIdCommand(
            Guid.CreateVersion7(),
            notification.PatientId,
            notification.FullName,
            notification.DateOfBirth,
            notification.Gender,
            notification.MothersFullName,
            notification.DocumentId,
            notification.Phone,
            notification.Email
        )).ConfigureAwait(false);
    }
}
```

- [ ] **Step 6: Run tests to confirm they pass**

```bash
dotnet test src/HC.LIS/HC.LIS.Modules/LabAnalysis/Tests/UnitTests/HC.LIS.Modules.LabAnalysis.UnitTests.csproj --filter "FullyQualifiedName~UpdatePatientSnapshot"
```

Expected: 2 tests pass.

- [ ] **Step 7: Commit**

```bash
git add "src/HC.LIS/HC.LIS.Modules/LabAnalysis/Tests/UnitTests/Patients/UpdatePatientSnapshotCommandHandlerTests.cs"
git commit -m "test(lab-analysis): update patient snapshot command handler and integration event handler"

git add "src/HC.LIS/HC.LIS.Modules/LabAnalysis/Application/Patients/UpdatePatientSnapshot/"
git commit -m "feat(lab-analysis): update patient snapshot on PatientUpdated event"
```

---

## Task 5: Anonymize Patient Snapshot (Command + Handler + Integration Event Handler)

**Files:**
- Create: `…/Application/Patients/AnonymizePatientSnapshot/AnonymizePatientSnapshotByPatientIdCommand.cs`
- Create: `…/Application/Patients/AnonymizePatientSnapshot/AnonymizePatientSnapshotByPatientIdCommandHandler.cs`
- Create: `…/Application/Patients/AnonymizePatientSnapshot/PatientAnonymizedIntegrationEventNotificationHandler.cs`
- Create: `…/Tests/UnitTests/Patients/AnonymizePatientSnapshotCommandHandlerTests.cs`

- [ ] **Step 1: Write the failing unit tests**

```csharp
using HC.LIS.Modules.LabAnalysis.Application.Patients;
using HC.LIS.Modules.LabAnalysis.Application.Patients.AnonymizePatientSnapshot;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.PatientManagement.IntegrationEvents;
using NSubstitute;

namespace HC.LIS.Modules.LabAnalysis.UnitTests.Patients;

public class AnonymizePatientSnapshotCommandHandlerTests
{
    private static readonly Guid PatientId = Guid.Parse("019b664c-52a4-7f37-a794-000000000003");
    private static readonly DateTime AnonymizedAt = new(2026, 6, 10, 14, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Handler_AnonymizesSnapshot()
    {
        IPatientSnapshotRepository repo = Substitute.For<IPatientSnapshotRepository>();
        var handler = new AnonymizePatientSnapshotByPatientIdCommandHandler(repo);
        var command = new AnonymizePatientSnapshotByPatientIdCommand(
            Guid.CreateVersion7(),
            PatientId,
            AnonymizedAt
        );

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        await repo.Received(1).AnonymizeAsync(PatientId, AnonymizedAt).ConfigureAwait(true);
    }

    [Fact]
    public async Task IntegrationEventHandler_EnqueuesAnonymizeCommand()
    {
        ICommandsScheduler scheduler = Substitute.For<ICommandsScheduler>();
        var handler = new PatientAnonymizedIntegrationEventNotificationHandler(scheduler);
        var notification = new PatientAnonymizedIntegrationEvent(
            Guid.CreateVersion7(),
            AnonymizedAt,
            PatientId,
            AnonymizedAt
        );

        await handler.Handle(notification, CancellationToken.None).ConfigureAwait(true);

        await scheduler.Received(1)
            .EnqueueAsync(Arg.Is<AnonymizePatientSnapshotByPatientIdCommand>(c =>
                c.PatientId == PatientId &&
                c.AnonymizedAt == AnonymizedAt
            )).ConfigureAwait(true);
    }
}
```

- [ ] **Step 2: Run tests to confirm they fail**

```bash
dotnet test src/HC.LIS/HC.LIS.Modules/LabAnalysis/Tests/UnitTests/HC.LIS.Modules.LabAnalysis.UnitTests.csproj --filter "FullyQualifiedName~AnonymizePatientSnapshot"
```

Expected: Compiler errors.

- [ ] **Step 3: Create the internal command**

```csharp
using System.Text.Json.Serialization;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Patients.AnonymizePatientSnapshot;

[method: JsonConstructor]
public class AnonymizePatientSnapshotByPatientIdCommand(
    Guid id,
    Guid patientId,
    DateTime anonymizedAt
) : InternalCommandBase(id)
{
    public Guid PatientId { get; } = patientId;
    public DateTime AnonymizedAt { get; } = anonymizedAt;
}
```

- [ ] **Step 4: Create the command handler**

```csharp
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Patients.AnonymizePatientSnapshot;

internal class AnonymizePatientSnapshotByPatientIdCommandHandler(
    IPatientSnapshotRepository patientSnapshotRepository
) : ICommandHandler<AnonymizePatientSnapshotByPatientIdCommand>
{
    private readonly IPatientSnapshotRepository _patientSnapshotRepository = patientSnapshotRepository;

    public async Task Handle(
        AnonymizePatientSnapshotByPatientIdCommand command,
        CancellationToken cancellationToken)
    {
        await _patientSnapshotRepository.AnonymizeAsync(
            command.PatientId,
            command.AnonymizedAt
        ).ConfigureAwait(false);
    }
}
```

- [ ] **Step 5: Create the integration event handler**

```csharp
using MediatR;
using HC.LIS.Modules.PatientManagement.IntegrationEvents;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Patients.AnonymizePatientSnapshot;

public class PatientAnonymizedIntegrationEventNotificationHandler(ICommandsScheduler commandsScheduler)
    : INotificationHandler<PatientAnonymizedIntegrationEvent>
{
    private readonly ICommandsScheduler _commandsScheduler = commandsScheduler;

    public async Task Handle(
        PatientAnonymizedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        await _commandsScheduler.EnqueueAsync(new AnonymizePatientSnapshotByPatientIdCommand(
            Guid.CreateVersion7(),
            notification.PatientId,
            notification.AnonymizedAt
        )).ConfigureAwait(false);
    }
}
```

- [ ] **Step 6: Run tests to confirm they pass**

```bash
dotnet test src/HC.LIS/HC.LIS.Modules/LabAnalysis/Tests/UnitTests/HC.LIS.Modules.LabAnalysis.UnitTests.csproj --filter "FullyQualifiedName~AnonymizePatientSnapshot"
```

Expected: 2 tests pass.

- [ ] **Step 7: Commit**

```bash
git add "src/HC.LIS/HC.LIS.Modules/LabAnalysis/Tests/UnitTests/Patients/AnonymizePatientSnapshotCommandHandlerTests.cs"
git commit -m "test(lab-analysis): anonymize patient snapshot command handler and integration event handler"

git add "src/HC.LIS/HC.LIS.Modules/LabAnalysis/Application/Patients/AnonymizePatientSnapshot/"
git commit -m "feat(lab-analysis): anonymize patient snapshot on PatientAnonymized event"
```

---

## Task 6: Infrastructure — PatientSnapshotRepository + DI Registration

**Files:**
- Create: `…/Infrastructure/Patients/PatientSnapshotRepository.cs`
- Modify: `…/Infrastructure/Configurations/DataAccess/DataAccessModule.cs`

- [ ] **Step 1: Create the Dapper repository**

```csharp
using System.Data;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.LabAnalysis.Application.Patients;

namespace HC.LIS.Modules.LabAnalysis.Infrastructure.Patients;

internal class PatientSnapshotRepository(ISqlConnectionFactory sqlConnectionFactory) : IPatientSnapshotRepository
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task StoreAsync(
        Guid patientId,
        string fullName,
        DateTime dateOfBirth,
        string? gender,
        string? mothersFullName,
        string? documentId,
        string? phone,
        string? email,
        DateTime registeredAt)
    {
        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to store patient snapshot");
        const string sql = """
            INSERT INTO "lab_analysis"."PatientSnapshotDetails"
                ("Id", "FullName", "DateOfBirth", "Gender", "MothersFullName", "DocumentId", "Phone", "Email", "Status", "RegisteredAt")
            VALUES
                (@PatientId, @FullName, @DateOfBirth, @Gender, @MothersFullName, @DocumentId, @Phone, @Email, 'Active', @RegisteredAt)
            """;
        await connection.ExecuteAsync(sql, new
        {
            PatientId = patientId,
            FullName = fullName,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            MothersFullName = mothersFullName,
            DocumentId = documentId,
            Phone = phone,
            Email = email,
            RegisteredAt = registeredAt
        }).ConfigureAwait(false);
    }

    public async Task UpdateAsync(
        Guid patientId,
        string fullName,
        DateTime dateOfBirth,
        string? gender,
        string? mothersFullName,
        string? documentId,
        string? phone,
        string? email)
    {
        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to update patient snapshot");
        const string sql = """
            UPDATE "lab_analysis"."PatientSnapshotDetails"
            SET "FullName" = @FullName,
                "DateOfBirth" = @DateOfBirth,
                "Gender" = @Gender,
                "MothersFullName" = @MothersFullName,
                "DocumentId" = @DocumentId,
                "Phone" = @Phone,
                "Email" = @Email
            WHERE "Id" = @PatientId
            """;
        await connection.ExecuteAsync(sql, new
        {
            PatientId = patientId,
            FullName = fullName,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            MothersFullName = mothersFullName,
            DocumentId = documentId,
            Phone = phone,
            Email = email
        }).ConfigureAwait(false);
    }

    public async Task AnonymizeAsync(Guid patientId, DateTime anonymizedAt)
    {
        IDbConnection connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to anonymize patient snapshot");
        const string sql = """
            UPDATE "lab_analysis"."PatientSnapshotDetails"
            SET "Status" = 'Anonymized',
                "AnonymizedAt" = @AnonymizedAt
            WHERE "Id" = @PatientId
            """;
        await connection.ExecuteAsync(sql, new
        {
            PatientId = patientId,
            AnonymizedAt = anonymizedAt
        }).ConfigureAwait(false);
    }
}
```

- [ ] **Step 2: Register in `DataAccessModule`**

In `DataAccessModule.cs`, add at the end of the `Load` method body, before the closing `}`:

```csharp
builder.RegisterType<PatientSnapshotRepository>()
    .As<IPatientSnapshotRepository>()
    .InstancePerLifetimeScope();
```

Also add the using at the top:
```csharp
using HC.LIS.Modules.LabAnalysis.Infrastructure.Patients;
using HC.LIS.Modules.LabAnalysis.Application.Patients;
```

- [ ] **Step 3: Verify build**

```bash
dotnet build src/HC.LIS/HC.LIS.Modules/LabAnalysis/Infrastructure/HC.LIS.Modules.LabAnalysis.Infrastructure.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Commit**

```bash
git add "src/HC.LIS/HC.LIS.Modules/LabAnalysis/Infrastructure/Patients/PatientSnapshotRepository.cs"
git add "src/HC.LIS/HC.LIS.Modules/LabAnalysis/Infrastructure/Configurations/DataAccess/DataAccessModule.cs"
git commit -m "feat(lab-analysis): implement PatientSnapshotRepository and register in DI"
```

---

## Task 7: Register Internal Commands in BiMap + Update TestBase

**Files:**
- Modify: `…/Infrastructure/Configurations/LabAnalysisStartup.cs`
- Modify: `…/Tests/IntegrationTests/TestBase.cs`

- [ ] **Step 1: Add 3 commands to the internal commands BiMap in `LabAnalysisStartup.cs`**

In `ConfigureContainer`, add three lines after `"CompleteWorklistItemBySignedReportCommand"`:

```csharp
internalCommandsMap.Add("StorePatientSnapshotByPatientIdCommand",    typeof(StorePatientSnapshotByPatientIdCommand));
internalCommandsMap.Add("UpdatePatientSnapshotByPatientIdCommand",   typeof(UpdatePatientSnapshotByPatientIdCommand));
internalCommandsMap.Add("AnonymizePatientSnapshotByPatientIdCommand", typeof(AnonymizePatientSnapshotByPatientIdCommand));
```

Add the three using directives at the top of the file:
```csharp
using HC.LIS.Modules.LabAnalysis.Application.Patients.StorePatientSnapshot;
using HC.LIS.Modules.LabAnalysis.Application.Patients.UpdatePatientSnapshot;
using HC.LIS.Modules.LabAnalysis.Application.Patients.AnonymizePatientSnapshot;
```

- [ ] **Step 2: Update `TestBase.ClearDatabase` to also clear `PatientSnapshotDetails`**

In `TestBase.cs`, add to the `ClearDatabase` SQL string:

```csharp
const string sql = @"DELETE FROM ""lab_analysis"".""InboxMessages"";
                     DELETE FROM ""lab_analysis"".""InternalCommands"";
                     DELETE FROM ""lab_analysis"".""OutboxMessages"";
                     DELETE FROM lab_analysis.worklist_item_analyte_results;
                     DELETE FROM lab_analysis.worklist_item_details;
                     DELETE FROM ""lab_analysis"".""PatientSnapshotDetails"";
                     DELETE FROM ""lab_analysis"".""mt_doc_deadletterevent"";
                     DELETE FROM ""lab_analysis"".""mt_event_progression"";
                     DELETE FROM ""lab_analysis"".""mt_events"";
                     DELETE FROM ""lab_analysis"".""mt_streams"";";
```

- [ ] **Step 3: Verify build**

```bash
dotnet build src/HC.LIS/HC.LIS.Modules/LabAnalysis/Infrastructure/HC.LIS.Modules.LabAnalysis.Infrastructure.csproj
```

Expected: Build succeeded.

- [ ] **Step 4: Commit**

```bash
git add "src/HC.LIS/HC.LIS.Modules/LabAnalysis/Infrastructure/Configurations/LabAnalysisStartup.cs"
git add "src/HC.LIS/HC.LIS.Modules/LabAnalysis/Tests/IntegrationTests/TestBase.cs"
git commit -m "feat(lab-analysis): register patient snapshot internal commands in BiMap"
```

---

## Task 8: Update Worklist List Query — DTO + Handler

**Files:**
- Modify: `…/Application/WorklistItems/GetWorklistItemList/WorklistItemSummaryDto.cs`
- Modify: `…/Application/WorklistItems/GetWorklistItemList/GetWorklistItemListQueryHandler.cs`
- Create: `…/Tests/IntegrationTests/WorklistItems/WorklistItemPatientInfoTests.cs` (list view assertions)

- [ ] **Step 1: Write the failing integration test**

```csharp
using Dapper;
using FluentAssertions;
using HC.Core.Domain;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemList;
using Npgsql;

namespace HC.LIS.Modules.LabAnalysis.IntegrationTests.WorklistItems;

public class WorklistItemPatientInfoTests : TestBase
{
    private static readonly Guid WorklistItemId = Guid.Parse("019b664c-0000-7f37-a794-000000000010");
    private static readonly Guid PatientId      = Guid.Parse("019b664c-0000-7f37-a794-000000000011");
    private static readonly DateTime DateOfBirth = new(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc);

    public WorklistItemPatientInfoTests() : base(Guid.CreateVersion7()) { }

    [Fact]
    public async Task WorklistList_IncludesPatientNameWhenSnapshotExists()
    {
        // Arrange: seed worklist item row and patient snapshot directly
        using var connection = new NpgsqlConnection(ConnectionString);
        await connection.ExecuteAsync("""
            INSERT INTO lab_analysis.worklist_item_details
                (id, sample_id, sample_barcode, exam_code, patient_id, order_id, order_item_id, status, created_at)
            VALUES
                (@Id, @SampleId, 'SC-PI-001', 'EXAM-001', @PatientId, @OrderId, @OrderItemId, 'Pending', @Now)
            """,
            new
            {
                Id = WorklistItemId,
                SampleId = Guid.CreateVersion7(),
                PatientId,
                OrderId = Guid.CreateVersion7(),
                OrderItemId = Guid.CreateVersion7(),
                Now = SystemClock.Now
            }).ConfigureAwait(true);

        await connection.ExecuteAsync("""
            INSERT INTO "lab_analysis"."PatientSnapshotDetails"
                ("Id", "FullName", "DateOfBirth", "Gender", "Status", "RegisteredAt")
            VALUES
                (@Id, @FullName, @DateOfBirth, @Gender, 'Active', @RegisteredAt)
            """,
            new
            {
                Id = PatientId,
                FullName = "John Doe",
                DateOfBirth,
                Gender = "Male",
                RegisteredAt = SystemClock.Now
            }).ConfigureAwait(true);

        // Act
        IReadOnlyCollection<WorklistItemSummaryDto> items = await LabAnalysisModule
            .ExecuteQueryAsync(new GetWorklistItemListQuery(null, 1, 20))
            .ConfigureAwait(true);

        // Assert
        WorklistItemSummaryDto item = items.Should().ContainSingle(i => i.Id == WorklistItemId).Subject;
        item.PatientName.Should().Be("John Doe");
        item.PatientDateOfBirth.Should().Be(DateOfBirth);
        item.PatientGender.Should().Be("Male");
    }

    [Fact]
    public async Task WorklistList_PatientFieldsAreNullWhenNoSnapshotExists()
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        await connection.ExecuteAsync("""
            INSERT INTO lab_analysis.worklist_item_details
                (id, sample_id, sample_barcode, exam_code, patient_id, order_id, order_item_id, status, created_at)
            VALUES
                (@Id, @SampleId, 'SC-PI-002', 'EXAM-002', @PatientId, @OrderId, @OrderItemId, 'Pending', @Now)
            """,
            new
            {
                Id = WorklistItemId,
                SampleId = Guid.CreateVersion7(),
                PatientId,
                OrderId = Guid.CreateVersion7(),
                OrderItemId = Guid.CreateVersion7(),
                Now = SystemClock.Now
            }).ConfigureAwait(true);

        IReadOnlyCollection<WorklistItemSummaryDto> items = await LabAnalysisModule
            .ExecuteQueryAsync(new GetWorklistItemListQuery(null, 1, 20))
            .ConfigureAwait(true);

        WorklistItemSummaryDto item = items.Should().ContainSingle(i => i.Id == WorklistItemId).Subject;
        item.PatientName.Should().BeNull();
        item.PatientDateOfBirth.Should().BeNull();
        item.PatientGender.Should().BeNull();
    }
}
```

- [ ] **Step 2: Run tests to confirm they fail**

```bash
dotnet test src/HC.LIS/HC.LIS.Modules/LabAnalysis/Tests/IntegrationTests/HC.LIS.Modules.LabAnalysis.IntegrationTests.csproj --filter "FullyQualifiedName~WorklistItemPatientInfo"
```

Expected: Compiler error — `PatientName`, `PatientDateOfBirth`, `PatientGender` don't exist on `WorklistItemSummaryDto`.

- [ ] **Step 3: Update `WorklistItemSummaryDto`**

```csharp
namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemList;

public record WorklistItemSummaryDto(
    Guid Id,
    string SampleBarcode,
    string ExamCode,
    Guid PatientId,
    string? PatientName,
    DateTime? PatientDateOfBirth,
    string? PatientGender,
    string Status,
    DateTime CreatedAt);
```

- [ ] **Step 4: Update `GetWorklistItemListQueryHandler` SQL**

Replace the `baseSql` constant:

```csharp
const string baseSql = $"""
    SELECT
        wid.id             AS "{nameof(WorklistItemSummaryDto.Id)}",
        wid.sample_barcode AS "{nameof(WorklistItemSummaryDto.SampleBarcode)}",
        wid.exam_code      AS "{nameof(WorklistItemSummaryDto.ExamCode)}",
        wid.patient_id     AS "{nameof(WorklistItemSummaryDto.PatientId)}",
        psd."FullName"     AS "{nameof(WorklistItemSummaryDto.PatientName)}",
        psd."DateOfBirth"  AS "{nameof(WorklistItemSummaryDto.PatientDateOfBirth)}",
        psd."Gender"       AS "{nameof(WorklistItemSummaryDto.PatientGender)}",
        wid.status         AS "{nameof(WorklistItemSummaryDto.Status)}",
        wid.created_at     AS "{nameof(WorklistItemSummaryDto.CreatedAt)}"
    FROM lab_analysis.worklist_item_details AS wid
    LEFT JOIN lab_analysis."PatientSnapshotDetails" AS psd ON psd."Id" = wid.patient_id
    WHERE (@Status IS NULL OR wid.status = @Status)
    ORDER BY wid.created_at
    """;
```

- [ ] **Step 5: Run tests to confirm they pass**

```bash
dotnet test src/HC.LIS/HC.LIS.Modules/LabAnalysis/Tests/IntegrationTests/HC.LIS.Modules.LabAnalysis.IntegrationTests.csproj --filter "FullyQualifiedName~WorklistItemPatientInfo"
```

Expected: 2 tests pass.

- [ ] **Step 6: Commit**

```bash
git add "src/HC.LIS/HC.LIS.Modules/LabAnalysis/Tests/IntegrationTests/WorklistItems/WorklistItemPatientInfoTests.cs"
git commit -m "test(lab-analysis): worklist list includes patient name, DOB, gender when snapshot exists"

git add "src/HC.LIS/HC.LIS.Modules/LabAnalysis/Application/WorklistItems/GetWorklistItemList/"
git commit -m "feat(lab-analysis): include patient info in worklist list query"
```

---

## Task 9: Update Worklist Detail Query — DTO + Handler

**Files:**
- Modify: `…/Application/WorklistItems/GetWorklistItemDetails/WorklistItemDetailsDto.cs`
- Modify: `…/Application/WorklistItems/GetWorklistItemDetails/GetWorklistItemDetailsQueryHandler.cs`

- [ ] **Step 1: Add failing assertion to `WorklistItemPatientInfoTests`**

Add this test to the existing `WorklistItemPatientInfoTests` class:

```csharp
[Fact]
public async Task WorklistDetail_IncludesPatientNameWhenSnapshotExists()
{
    using var connection = new NpgsqlConnection(ConnectionString);
    await connection.ExecuteAsync("""
        INSERT INTO lab_analysis.worklist_item_details
            (id, sample_id, sample_barcode, exam_code, patient_id, order_id, order_item_id, status, created_at)
        VALUES
            (@Id, @SampleId, 'SC-PI-003', 'EXAM-003', @PatientId, @OrderId, @OrderItemId, 'Pending', @Now)
        """,
        new
        {
            Id = WorklistItemId,
            SampleId = Guid.CreateVersion7(),
            PatientId,
            OrderId = Guid.CreateVersion7(),
            OrderItemId = Guid.CreateVersion7(),
            Now = SystemClock.Now
        }).ConfigureAwait(true);

    await connection.ExecuteAsync("""
        INSERT INTO "lab_analysis"."PatientSnapshotDetails"
            ("Id", "FullName", "DateOfBirth", "Gender", "Status", "RegisteredAt")
        VALUES
            (@Id, @FullName, @DateOfBirth, @Gender, 'Active', @RegisteredAt)
        """,
        new
        {
            Id = PatientId,
            FullName = "John Doe",
            DateOfBirth,
            Gender = "Male",
            RegisteredAt = SystemClock.Now
        }).ConfigureAwait(true);

    WorklistItemDetailsDto? dto = await LabAnalysisModule
        .ExecuteQueryAsync(new GetWorklistItemDetailsQuery(WorklistItemId))
        .ConfigureAwait(true);

    dto.Should().NotBeNull();
    dto!.PatientName.Should().Be("John Doe");
    dto.PatientDateOfBirth.Should().Be(DateOfBirth);
    dto.PatientGender.Should().Be("Male");
}
```

- [ ] **Step 2: Run test to confirm it fails**

```bash
dotnet test src/HC.LIS/HC.LIS.Modules/LabAnalysis/Tests/IntegrationTests/HC.LIS.Modules.LabAnalysis.IntegrationTests.csproj --filter "WorklistDetail_IncludesPatientName"
```

Expected: Compiler error — properties not on `WorklistItemDetailsDto`.

- [ ] **Step 3: Update `WorklistItemDetailsDto`**

Add three properties after `PatientId`:

```csharp
public class WorklistItemDetailsDto
{
    public Guid Id { get; set; }
    public Guid SampleId { get; set; }
    public string SampleBarcode { get; set; } = string.Empty;
    public string ExamCode { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public string? PatientName { get; set; }
    public DateTime? PatientDateOfBirth { get; set; }
    public string? PatientGender { get; set; }
    public Guid OrderId { get; set; }
    public Guid OrderItemId { get; set; }
    public string Status { get; set; } = string.Empty;
    public IReadOnlyCollection<AnalyteResultDto> AnalyteResults { get; set; } = Array.Empty<AnalyteResultDto>();
    public string? ReportPath { get; set; }
    public string? CompletionType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
```

- [ ] **Step 4: Update `GetWorklistItemDetailsQueryHandler` SQL**

Replace the first SELECT block in the `sql` constant to add the LEFT JOIN and three columns:

```csharp
const string sql = @"
    SELECT
        wid.id               AS ""Id"",
        wid.sample_id        AS ""SampleId"",
        wid.sample_barcode   AS ""SampleBarcode"",
        wid.exam_code        AS ""ExamCode"",
        wid.patient_id       AS ""PatientId"",
        psd.""FullName""     AS ""PatientName"",
        psd.""DateOfBirth""  AS ""PatientDateOfBirth"",
        psd.""Gender""       AS ""PatientGender"",
        wid.order_id         AS ""OrderId"",
        wid.order_item_id    AS ""OrderItemId"",
        wid.status           AS ""Status"",
        wid.report_path      AS ""ReportPath"",
        wid.completion_type  AS ""CompletionType"",
        wid.created_at       AS ""CreatedAt"",
        wid.completed_at     AS ""CompletedAt""
    FROM lab_analysis.worklist_item_details AS wid
    LEFT JOIN lab_analysis.""PatientSnapshotDetails"" AS psd ON psd.""Id"" = wid.patient_id
    WHERE wid.id = @WorklistItemId;

    SELECT
        r.id              AS ""Id"",
        r.analyte_code    AS ""AnalyteCode"",
        r.result_value    AS ""ResultValue"",
        r.result_unit     AS ""ResultUnit"",
        r.reference_range  AS ""ReferenceRange"",
        r.is_out_of_range  AS ""IsOutOfRange"",
        r.performed_by_id  AS ""PerformedById"",
        r.recorded_at     AS ""RecordedAt""
    FROM lab_analysis.worklist_item_analyte_results AS r
    WHERE r.worklist_item_id = @WorklistItemId
    ORDER BY r.recorded_at;";
```

- [ ] **Step 5: Run tests to confirm they pass**

```bash
dotnet test src/HC.LIS/HC.LIS.Modules/LabAnalysis/Tests/IntegrationTests/HC.LIS.Modules.LabAnalysis.IntegrationTests.csproj --filter "FullyQualifiedName~WorklistItemPatientInfo"
```

Expected: All 3 patient info tests pass.

- [ ] **Step 6: Run all LabAnalysis tests to check for regressions**

```bash
dotnet test src/HC.LIS/HC.LIS.Modules/LabAnalysis/Tests/IntegrationTests/HC.LIS.Modules.LabAnalysis.IntegrationTests.csproj
dotnet test src/HC.LIS/HC.LIS.Modules/LabAnalysis/Tests/UnitTests/HC.LIS.Modules.LabAnalysis.UnitTests.csproj
```

Expected: All tests pass.

- [ ] **Step 7: Commit**

```bash
git add "src/HC.LIS/HC.LIS.Modules/LabAnalysis/Tests/IntegrationTests/WorklistItems/WorklistItemPatientInfoTests.cs"
git commit -m "test(lab-analysis): worklist detail includes patient info when snapshot exists"

git add "src/HC.LIS/HC.LIS.Modules/LabAnalysis/Application/WorklistItems/GetWorklistItemDetails/"
git commit -m "feat(lab-analysis): include patient info in worklist detail query"
```

---

## Task 10: Frontend — TypeScript Interfaces + Angular Templates

**Files:**
- Modify: `src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/core/domain/worklist-item-summary.ts`
- Modify: `src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/core/domain/worklist-item-details.ts`
- Modify: `src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/features/worklist/worklist.component.ts`
- Modify: `src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/features/worklist/worklist-item-detail.component.ts`

- [ ] **Step 1: Update `worklist-item-summary.ts`**

```typescript
export interface WorklistItemSummary {
  id: string;
  sampleBarcode: string;
  examCode: string;
  patientId: string;
  patientName: string | null;
  patientDateOfBirth: string | null;
  patientGender: string | null;
  status: string;
  createdAt: string;
}
```

- [ ] **Step 2: Update `worklist-item-details.ts`**

```typescript
export interface AnalyteResult {
  id: string;
  analyteCode: string;
  resultValue: string;
  resultUnit: string;
  referenceRange: string;
  isOutOfRange: boolean;
  performedById: string;
  recordedAt: string;
}

export interface WorklistItemDetails {
  id: string;
  sampleId: string;
  sampleBarcode: string;
  examCode: string;
  patientId: string;
  patientName: string | null;
  patientDateOfBirth: string | null;
  patientGender: string | null;
  orderId: string;
  orderItemId: string;
  status: string;
  analyteResults: AnalyteResult[];
  reportPath: string | null;
  completionType: string | null;
  createdAt: string;
  completedAt: string | null;
}
```

- [ ] **Step 3: Update `worklist.component.ts` — replace `Patient ID` column**

In the table `<thead>`, replace:
```html
<th>Patient ID</th>
```
with:
```html
<th>Patient</th>
```

In the table `<tbody>` row, replace:
```html
<td>{{ item.patientId }}</td>
```
with:
```html
<td data-testid="patient-name-cell">{{ item.patientName ?? item.patientId }}</td>
```

- [ ] **Step 4: Update `worklist-item-detail.component.ts` — replace patient GUID**

Replace:
```html
<p>Patient ID: {{ item.patientId }}</p>
```
with:
```html
<p data-testid="patient-name">Patient: {{ item.patientName ?? item.patientId }}</p>
@if (item.patientDateOfBirth) {
  <p data-testid="patient-dob">Date of birth: {{ item.patientDateOfBirth | date:'shortDate' }}</p>
}
@if (item.patientGender) {
  <p data-testid="patient-gender">Gender: {{ item.patientGender }}</p>
}
```

Also add `DatePipe` to the `imports` array since `date` pipe is used:
```typescript
import { DatePipe } from '@angular/common';
// ...
imports: [FormsModule, DatePipe],
```

- [ ] **Step 5: Build the frontend to check for type errors**

```bash
cd src/HC.LIS.Frontend/packages/hc-lis-spa && yarn build 2>&1 | tail -20
```

Expected: Build succeeded, no TypeScript errors.

- [ ] **Step 6: Commit**

```bash
git add src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/core/domain/worklist-item-summary.ts
git add src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/core/domain/worklist-item-details.ts
git add src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/features/worklist/worklist.component.ts
git add src/HC.LIS.Frontend/packages/hc-lis-spa/src/app/features/worklist/worklist-item-detail.component.ts
git commit -m "feat(worklist): display patient name, DOB and gender instead of GUID"
```

---

## Task 11: E2E Test Update

**Files:**
- Modify: `src/HC.LIS.Frontend/packages/hc-lis-spa/e2e/worklist.spec.ts`

- [ ] **Step 1: Add `data-testid` assertion for patient name in the `fixme` test**

In `worklist.spec.ts`, update the fixme test to also assert patient name is shown:

After `await expect(page.getByTestId('worklist-item-detail')).toBeVisible(...)`, add:

```typescript
await expect(page.getByTestId('patient-name')).not.toContainText('-'); // should be a name, not a GUID
```

And after the row is visible, add an assertion that the patient cell doesn't show a UUID pattern:

```typescript
const patientCell = page.getByTestId('patient-name-cell').first();
await expect(patientCell).not.toContainText(/^[0-9a-f]{8}-/i);
```

- [ ] **Step 2: Commit**

```bash
git add src/HC.LIS.Frontend/packages/hc-lis-spa/e2e/worklist.spec.ts
git commit -m "test(e2e): assert patient name displayed in worklist (not GUID)"
```

---

## Final Verification

- [ ] Run all LabAnalysis tests end-to-end:
  ```bash
  dotnet test src/HC.LIS/HC.LIS.Modules/LabAnalysis/Tests/UnitTests/HC.LIS.Modules.LabAnalysis.UnitTests.csproj
  dotnet test src/HC.LIS/HC.LIS.Modules/LabAnalysis/Tests/IntegrationTests/HC.LIS.Modules.LabAnalysis.IntegrationTests.csproj
  ```
- [ ] Start the API + DB, register a patient, create a test order, and verify `lab_analysis."PatientSnapshotDetails"` is populated (check with a DB client).
- [ ] Open `http://localhost:4200`, log in as `physician@hclis.local / Admin1234!`, navigate to the worklist — confirm patient name appears in the table, not a GUID.
- [ ] Click a row — confirm the detail panel shows name, date of birth, and gender.
