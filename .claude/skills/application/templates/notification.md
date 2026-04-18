# Notification + NotificationProjection + PublishEventNotificationHandler + IntegrationEventNotificationHandler — pattern reference

## When to read
Creating any of: a `DomainNotificationBase<T>` subclass, a projection-dispatch handler, a publish-to-event-bus handler, or a handler that schedules an internal command when an integration event fires.

## File locations
```
Application/{Aggregate}/{UseCaseName}/{Event}Notification.cs
Application/{Aggregate}/{UseCaseName}/{Event}NotificationProjection.cs
Application/{Aggregate}/{UseCaseName}/{Event}PublishEventNotificationHandler.cs
```
The IntegrationEventNotificationHandler that schedules a command lives in the **target command's folder**:
```
Application/{Aggregate}/{TargetUseCaseName}/{ExternalEvent}IntegrationEventNotificationHandler.cs
```

## Naming rules

| File | Class name |
|---|---|
| `{Event}Notification.cs` | `{Event}Notification` |
| `{Event}NotificationProjection.cs` | `{Event}NotificationProjection` |
| `{Event}PublishEventNotificationHandler.cs` | `{Event}PublishEventNotificationHandler` |
| `{ExternalEvent}IntegrationEventNotificationHandler.cs` | `{ExternalEvent}IntegrationEventNotificationHandler` |

CA1711: class names must NOT end in `EventHandler` — use `NotificationHandler` suffix.

## Notification (DomainNotificationBase subclass)
```csharp
using HC.Core.Application.Events;
using HC.LIS.Modules.{ModuleName}.Domain.{Aggregate}.Events;

namespace HC.LIS.Modules.{ModuleName}.Application.{Aggregate}.{UseCaseName};

public class {Event}Notification({Event}DomainEvent domainEvent, Guid id)
    : DomainNotificationBase<{Event}DomainEvent>(domainEvent, id)
{
}
```

## NotificationProjection (dispatches to projectors)
```csharp
using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.{ModuleName}.Application.{Aggregate}.{UseCaseName};

public class {Event}NotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<{Event}Notification>
{
    private readonly IList<IProjector> _projectors = projectors;

    public async Task Handle(
        {Event}Notification notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
```

## PublishEventNotificationHandler (fires integration event)
```csharp
using MediatR;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.{ModuleName}.IntegrationEvents;

namespace HC.LIS.Modules.{ModuleName}.Application.{Aggregate}.{UseCaseName};

public class {Event}PublishEventNotificationHandler(IEventsBus eventsBus)
    : INotificationHandler<{Event}Notification>
{
    private readonly IEventsBus _eventsBus = eventsBus;

    public async Task Handle(
        {Event}Notification notification,
        CancellationToken cancellationToken
    )
    {
        await _eventsBus.Publish(new {Event}IntegrationEvent(
            notification.Id,
            notification.DomainEvent.OcurredAt,
            notification.DomainEvent.{Prop1},
            notification.DomainEvent.{Prop2}
        )).ConfigureAwait(false);
    }
}
```

## IntegrationEventNotificationHandler (schedules internal command)
Place in the **same folder as the command it schedules**.
```csharp
using MediatR;
using HC.LIS.Modules.{ModuleName}.Application.Configuration.Commands;
using HC.LIS.Modules.{ProducerModule}.IntegrationEvents;

namespace HC.LIS.Modules.{ModuleName}.Application.{Aggregate}.{TargetUseCaseName};

public class {ExternalEvent}IntegrationEventNotificationHandler(ICommandsScheduler commandsScheduler)
    : INotificationHandler<{ExternalEvent}IntegrationEvent>
{
    private readonly ICommandsScheduler _commandsScheduler = commandsScheduler;

    public async Task Handle(
        {ExternalEvent}IntegrationEvent notification,
        CancellationToken cancellationToken
    )
    {
        await _commandsScheduler.EnqueueAsync(new {Action}By{Key}Command(
            Guid.CreateVersion7(),
            notification.{Prop1},
            notification.OccurredAt
        )).ConfigureAwait(false);
    }
}
```

## IRON RULES
- Access domain event properties **directly inline**: `notification.DomainEvent.OrderId` — never `var ev = notification.DomainEvent; ev.OrderId`.
- `Notification`, `NotificationProjection`, `PublishEventNotificationHandler`, and `IntegrationEventNotificationHandler` are all `public`.
- `IntegrationEventNotificationHandler` lives in the **command's folder**, never in the event's folder.
- `.ConfigureAwait(false)` on every `await`.

## References
- `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/AcceptExam/ExamAcceptedNotification.cs`
- `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/AcceptExam/ExamAcceptedNotificationProjection.cs`
- `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/RequestExam/ExamRequestedPublishEventNotificationHandler.cs`
- `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/PlaceExamInProgress/SampleCollectedIntegrationEventHandler.cs`
- `src/HC.LIS/HC.LIS.Modules/TestOrders/Application/Orders/CompleteExam/WorklistItemCompletedIntegrationEventNotificationHandler.cs`
