# PatientManagement Phase 3 — Application Layer: Commands & Notifications

> **Status: COMPLETED 2026-05-27** — All 6 tasks implemented via subagent-driven development. 5/5 unit tests pass. 0 errors, 0 warnings.
>
> **Commits:** `9a2847e` → `f83b06e` → `231f56f` → `e10af9f` → `c6d17ba` → `b2bbd07` → `7aaf9d5` → `d83b429`
>
> **Note added during implementation:** BiMap registrations for all 3 notifications were added to `PatientManagementStartup.cs` (commits `c6d17ba` and `d83b429`) — this was originally planned for Phase 6 Task 6.2 but done eagerly here to prevent startup crash.

**Goal:** Add commands, handlers, notifications, and notification projections for the three patient lifecycle operations (Register, Update, Anonymize) in the PatientManagement Application layer.

**Architecture:** Each operation gets a Command + Handler (CQRS write side) and a Notification + NotificationProjection (MediatR notification pipeline), all colocated in the same feature subfolder under `Application/Patients/`. Handlers delegate to the `Patient` aggregate. Projectors that write to read models are wired in Phase 4 — the NotificationProjection here just calls whatever `IProjector` implementations are registered.

**Tech Stack:** .NET 10, C# 13, MediatR, `IAggregateStore` (Marten-backed), `ICommandHandler<T>`, `DomainNotificationBase<T>`, `INotificationHandler<T>`, FluentAssertions (for build verification)

---

## Files Created

```
src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/Patients/
├── RegisterPatient/
│   ├── RegisterPatientCommand.cs
│   ├── RegisterPatientCommandHandler.cs
│   ├── PatientRegisteredNotification.cs
│   └── PatientRegisteredNotificationProjection.cs
├── UpdatePatient/
│   ├── UpdatePatientCommand.cs
│   ├── UpdatePatientCommandHandler.cs
│   ├── PatientUpdatedNotification.cs
│   └── PatientUpdatedNotificationProjection.cs
└── AnonymizePatient/
    ├── AnonymizePatientCommand.cs
    ├── AnonymizePatientCommandHandler.cs
    ├── PatientAnonymizedNotification.cs
    └── PatientAnonymizedNotificationProjection.cs
```

**Key references confirmed from codebase:**
- `CommandBase<Guid>` / `CommandBase` — `Application/Contracts/CommandBase.cs`
- `ICommandHandler<TCommand, TResult>` / `ICommandHandler<TCommand>` — `Application/Configuration/Commands/ICommandHandler.cs`
- `IAggregateStore` — `HC.Core.Domain.EventSourcing`
- `InvalidCommandException` — `HC.Core.Application`
- `DomainNotificationBase<T>` — `HC.Core.Application.Events`
- `IProjector` — `HC.Core.Application.Projections`
- `INotificationHandler<T>` — `MediatR`
- `PatientId(Guid)` — `Domain/Patients/PatientId.cs` (`AggregateId<Patient>`)

---

## Task 1: RegisterPatient Command and Handler (Task 3.1)

**Files:**
- Create: `src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/Patients/RegisterPatient/RegisterPatientCommand.cs`
- Create: `src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/Patients/RegisterPatient/RegisterPatientCommandHandler.cs`

- [x] **Step 1: Create RegisterPatientCommand.cs**

```csharp
using HC.LIS.Modules.PatientManagement.Application.Contracts;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.RegisterPatient;

public class RegisterPatientCommand(
    Guid patientId,
    string fullName,
    DateTime dateOfBirth,
    string? gender,
    string? mothersFullName,
    string? documentId,
    string? phone,
    string? email,
    DateTime registeredAt
) : CommandBase<Guid>
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

- [x] **Step 2: Create RegisterPatientCommandHandler.cs**

`Patient.Register` takes `Guid id` directly (not `PatientId`). Use `_aggregateStore.Start()` for new aggregates (no Load needed).

```csharp
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.PatientManagement.Application.Configuration.Commands;
using HC.LIS.Modules.PatientManagement.Domain.Patients;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.RegisterPatient;

internal class RegisterPatientCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<RegisterPatientCommand, Guid>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task<Guid> Handle(
        RegisterPatientCommand command,
        CancellationToken cancellationToken
    )
    {
        Patient patient = Patient.Register(
            command.PatientId,
            command.FullName,
            command.DateOfBirth,
            command.Gender,
            command.MothersFullName,
            command.DocumentId,
            command.Phone,
            command.Email,
            command.RegisteredAt
        );

        _aggregateStore.Start(patient);
        return patient.Id;
    }
}
```

- [x] **Step 3: Verify build**

Run: `dotnet build src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/HC.LIS.Modules.PatientManagement.Application.csproj`

Expected: `Build succeeded` with 0 errors

- [x] **Step 4: Commit**

```
git add src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/Patients/RegisterPatient/RegisterPatientCommand.cs
git add src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/Patients/RegisterPatient/RegisterPatientCommandHandler.cs
git commit -m "feat(patient-management): implement RegisterPatientCommand and handler"
```

---

## Task 2: PatientRegistered Notification and Projection (Task 3.2)

**Files:**
- Create: `src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/Patients/RegisterPatient/PatientRegisteredNotification.cs`
- Create: `src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/Patients/RegisterPatient/PatientRegisteredNotificationProjection.cs`

- [x] **Step 1: Create PatientRegisteredNotification.cs**

```csharp
using HC.Core.Application.Events;
using HC.LIS.Modules.PatientManagement.Domain.Patients.Events;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.RegisterPatient;

public class PatientRegisteredNotification(PatientRegisteredDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<PatientRegisteredDomainEvent>(domainEvent, id)
{

}
```

- [x] **Step 2: Create PatientRegisteredNotificationProjection.cs**

```csharp
using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.RegisterPatient;

public class PatientRegisteredNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<PatientRegisteredNotification>
{
    private readonly IList<IProjector> _projectors = projectors;

    public async Task Handle(
        PatientRegisteredNotification notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
```

- [x] **Step 3: Verify build**

Run: `dotnet build src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/HC.LIS.Modules.PatientManagement.Application.csproj`

Expected: `Build succeeded` with 0 errors

- [x] **Step 4: Commit**

```
git add src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/Patients/RegisterPatient/PatientRegisteredNotification.cs
git add src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/Patients/RegisterPatient/PatientRegisteredNotificationProjection.cs
git commit -m "feat(patient-management): implement PatientRegisteredNotification and projection"
```

---

## Task 3: UpdatePatient Command and Handler (Task 3.3)

**Files:**
- Create: `src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/Patients/UpdatePatient/UpdatePatientCommand.cs`
- Create: `src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/Patients/UpdatePatient/UpdatePatientCommandHandler.cs`

- [x] **Step 1: Create UpdatePatientCommand.cs**

Extends `CommandBase` (no return value — update operations don't return the ID).

```csharp
using HC.LIS.Modules.PatientManagement.Application.Contracts;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.UpdatePatient;

public class UpdatePatientCommand(
    Guid patientId,
    string fullName,
    DateTime dateOfBirth,
    string? gender,
    string? mothersFullName,
    string? documentId,
    string? phone,
    string? email,
    DateTime updatedAt
) : CommandBase
{
    public Guid PatientId { get; } = patientId;
    public string FullName { get; } = fullName;
    public DateTime DateOfBirth { get; } = dateOfBirth;
    public string? Gender { get; } = gender;
    public string? MothersFullName { get; } = mothersFullName;
    public string? DocumentId { get; } = documentId;
    public string? Phone { get; } = phone;
    public string? Email { get; } = email;
    public DateTime UpdatedAt { get; } = updatedAt;
}
```

- [x] **Step 2: Create UpdatePatientCommandHandler.cs**

Load existing aggregate with `new PatientId(command.PatientId)`, call `patient.Update(...)`, then `AppendChanges`.

```csharp
using HC.Core.Application;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.PatientManagement.Application.Configuration.Commands;
using HC.LIS.Modules.PatientManagement.Domain.Patients;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.UpdatePatient;

internal class UpdatePatientCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<UpdatePatientCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task Handle(
        UpdatePatientCommand command,
        CancellationToken cancellationToken
    )
    {
        Patient? patient = await _aggregateStore.Load(new PatientId(command.PatientId)).ConfigureAwait(false) ??
            throw new InvalidCommandException("Patient must exist to update");

        patient.Update(
            command.FullName,
            command.DateOfBirth,
            command.Gender,
            command.MothersFullName,
            command.DocumentId,
            command.Phone,
            command.Email,
            command.UpdatedAt
        );

        _aggregateStore.AppendChanges(patient);
    }
}
```

- [x] **Step 3: Verify build**

Run: `dotnet build src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/HC.LIS.Modules.PatientManagement.Application.csproj`

Expected: `Build succeeded` with 0 errors

- [x] **Step 4: Commit**

```
git add src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/Patients/UpdatePatient/UpdatePatientCommand.cs
git add src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/Patients/UpdatePatient/UpdatePatientCommandHandler.cs
git commit -m "feat(patient-management): implement UpdatePatientCommand and handler"
```

---

## Task 4: PatientUpdated Notification and Projection (Task 3.4)

**Files:**
- Create: `src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/Patients/UpdatePatient/PatientUpdatedNotification.cs`
- Create: `src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/Patients/UpdatePatient/PatientUpdatedNotificationProjection.cs`

- [x] **Step 1: Create PatientUpdatedNotification.cs**

```csharp
using HC.Core.Application.Events;
using HC.LIS.Modules.PatientManagement.Domain.Patients.Events;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.UpdatePatient;

public class PatientUpdatedNotification(PatientUpdatedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<PatientUpdatedDomainEvent>(domainEvent, id)
{

}
```

- [x] **Step 2: Create PatientUpdatedNotificationProjection.cs**

```csharp
using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.UpdatePatient;

public class PatientUpdatedNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<PatientUpdatedNotification>
{
    private readonly IList<IProjector> _projectors = projectors;

    public async Task Handle(
        PatientUpdatedNotification notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
```

- [x] **Step 3: Verify build**

Run: `dotnet build src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/HC.LIS.Modules.PatientManagement.Application.csproj`

Expected: `Build succeeded` with 0 errors

- [x] **Step 4: Commit**

```
git add src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/Patients/UpdatePatient/PatientUpdatedNotification.cs
git add src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/Patients/UpdatePatient/PatientUpdatedNotificationProjection.cs
git commit -m "feat(patient-management): implement PatientUpdatedNotification and projection"
```

---

## Task 5: AnonymizePatient Command and Handler (Task 3.5)

**Files:**
- Create: `src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/Patients/AnonymizePatient/AnonymizePatientCommand.cs`
- Create: `src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/Patients/AnonymizePatient/AnonymizePatientCommandHandler.cs`

- [x] **Step 1: Create AnonymizePatientCommand.cs**

Simpler than Update — only `PatientId` and `AnonymizedAt` are needed.

```csharp
using HC.LIS.Modules.PatientManagement.Application.Contracts;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.AnonymizePatient;

public class AnonymizePatientCommand(
    Guid patientId,
    DateTime anonymizedAt
) : CommandBase
{
    public Guid PatientId { get; } = patientId;
    public DateTime AnonymizedAt { get; } = anonymizedAt;
}
```

- [x] **Step 2: Create AnonymizePatientCommandHandler.cs**

```csharp
using HC.Core.Application;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.PatientManagement.Application.Configuration.Commands;
using HC.LIS.Modules.PatientManagement.Domain.Patients;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.AnonymizePatient;

internal class AnonymizePatientCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<AnonymizePatientCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task Handle(
        AnonymizePatientCommand command,
        CancellationToken cancellationToken
    )
    {
        Patient? patient = await _aggregateStore.Load(new PatientId(command.PatientId)).ConfigureAwait(false) ??
            throw new InvalidCommandException("Patient must exist to anonymize");

        patient.Anonymize(command.AnonymizedAt);

        _aggregateStore.AppendChanges(patient);
    }
}
```

- [x] **Step 3: Verify build**

Run: `dotnet build src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/HC.LIS.Modules.PatientManagement.Application.csproj`

Expected: `Build succeeded` with 0 errors

- [x] **Step 4: Commit**

```
git add src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/Patients/AnonymizePatient/AnonymizePatientCommand.cs
git add src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/Patients/AnonymizePatient/AnonymizePatientCommandHandler.cs
git commit -m "feat(patient-management): implement AnonymizePatientCommand and handler"
```

---

## Task 6: PatientAnonymized Notification and Projection (Task 3.6)

**Files:**
- Create: `src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/Patients/AnonymizePatient/PatientAnonymizedNotification.cs`
- Create: `src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/Patients/AnonymizePatient/PatientAnonymizedNotificationProjection.cs`

- [x] **Step 1: Create PatientAnonymizedNotification.cs**

```csharp
using HC.Core.Application.Events;
using HC.LIS.Modules.PatientManagement.Domain.Patients.Events;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.AnonymizePatient;

public class PatientAnonymizedNotification(PatientAnonymizedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<PatientAnonymizedDomainEvent>(domainEvent, id)
{

}
```

- [x] **Step 2: Create PatientAnonymizedNotificationProjection.cs**

```csharp
using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.AnonymizePatient;

public class PatientAnonymizedNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<PatientAnonymizedNotification>
{
    private readonly IList<IProjector> _projectors = projectors;

    public async Task Handle(
        PatientAnonymizedNotification notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
```

- [x] **Step 3: Verify build**

Run: `dotnet build src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/HC.LIS.Modules.PatientManagement.Application.csproj`

Expected: `Build succeeded` with 0 errors

- [x] **Step 4: Commit**

```
git add src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/Patients/AnonymizePatient/PatientAnonymizedNotification.cs
git add src/HC.LIS/HC.LIS.Modules/PatientManagement/Application/Patients/AnonymizePatient/PatientAnonymizedNotificationProjection.cs
git commit -m "feat(patient-management): implement PatientAnonymizedNotification and projection"
```

---

## Verification ✅

Application project: `Build succeeded. 0 Warning(s), 0 Error(s)`
Infrastructure project: `Build succeeded. 0 Warning(s), 0 Error(s)`
Unit tests: `Passed! — Failed: 0, Passed: 5, Skipped: 0`

Task breakdown updated: `docs/specs/PatientManagement-Tasks.md` — all Phase 3 tasks marked `[x]`.
