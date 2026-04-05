# Integration event handler test template

Handler tests are **standalone** — do NOT extend `TestBase`.
Use `.ConfigureAwait(true)` in test code (opposite of production code's `.ConfigureAwait(false)`).
Use `notification.{SomeProperty}` inline — no intermediate variable for the event payload.

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using HC.LIS.Modules.{SourceModule}.IntegrationEvents;
using HC.LIS.Modules.{ModuleName}.Application.Configuration.Commands;
using HC.LIS.Modules.{ModuleName}.Application.{Aggregate}s.{Action};
using NSubstitute;
using Xunit;

namespace HC.LIS.Modules.{ModuleName}.UnitTests.{Aggregate}s;

public class {Event}IntegrationEventNotificationHandlerTests
{
    [Fact]
    public async Task HandleEnqueues{CommandName}()
    {
        var scheduler = Substitute.For<ICommandsScheduler>();
        var handler = new {Event}IntegrationEventNotificationHandler(scheduler);
        var occurredAt = DateTime.UtcNow;
        var notification = new {Event}IntegrationEvent(
            Guid.CreateVersion7(),
            occurredAt,
            {SampleData}.{Prop1},
            {SampleData}.{Prop2}
        );

        await handler.Handle(notification, CancellationToken.None).ConfigureAwait(true);

        await scheduler.Received(1).EnqueueAsync(
            Arg.Is<{CommandName}>(cmd =>
                cmd.{Prop1} == {SampleData}.{Prop1} &&
                cmd.{Prop2} == occurredAt)).ConfigureAwait(true);
    }
}
```

## Canonical example

`src/HC.LIS/HC.LIS.Modules/TestOrders/Tests/UnitTests/Orders/WorklistItemCompletedIntegrationEventNotificationHandlerTests.cs`
