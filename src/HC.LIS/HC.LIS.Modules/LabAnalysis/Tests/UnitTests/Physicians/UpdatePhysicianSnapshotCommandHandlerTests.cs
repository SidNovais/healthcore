using System;
using System.Threading;
using System.Threading.Tasks;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.LabAnalysis.Application.Physicians;
using HC.LIS.Modules.LabAnalysis.Application.Physicians.UpdatePhysicianSnapshot;
using HC.LIS.Modules.TestOrders.IntegrationEvents;
using NSubstitute;
using Xunit;

namespace HC.LIS.Modules.LabAnalysis.UnitTests.Physicians;

public class UpdatePhysicianSnapshotCommandHandlerTests
{
    private static readonly Guid PhysicianId = Guid.Parse("019b664c-52a4-7f37-a794-000000000101");
    private static readonly DateTime UpdatedAt = new(2026, 8, 6, 9, 30, 0, DateTimeKind.Utc);

    [Fact]
    public async Task HandlerUpdatesSnapshot()
    {
        IPhysicianSnapshotRepository repo = Substitute.For<IPhysicianSnapshotRepository>();
        var handler = new UpdatePhysicianSnapshotByPhysicianIdCommandHandler(repo);
        var command = new UpdatePhysicianSnapshotByPhysicianIdCommand(
            Guid.CreateVersion7(),
            PhysicianId,
            "Ana Lima Souza",
            "CRM-99999",
            UpdatedAt
        );

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        await repo.Received(1).UpdateAsync(
            PhysicianId,
            "Ana Lima Souza",
            "CRM-99999",
            UpdatedAt
        ).ConfigureAwait(true);
    }

    [Fact]
    public async Task IntegrationEventHandlerEnqueuesUpdateCommand()
    {
        ICommandsScheduler scheduler = Substitute.For<ICommandsScheduler>();
        var handler = new PhysicianUpdatedIntegrationEventNotificationHandler(scheduler);
        var notification = new PhysicianUpdatedIntegrationEvent(
            Guid.CreateVersion7(),
            UpdatedAt,
            PhysicianId,
            "Ana Lima Souza",
            "CRM-99999",
            UpdatedAt
        );

        await handler.Handle(notification, CancellationToken.None).ConfigureAwait(true);

        await scheduler.Received(1)
            .EnqueueAsync(Arg.Is<UpdatePhysicianSnapshotByPhysicianIdCommand>(c =>
                c.PhysicianId == PhysicianId &&
                c.FullName == "Ana Lima Souza" &&
                c.LicenceNumber == "CRM-99999" &&
                c.UpdatedAt == UpdatedAt
            )).ConfigureAwait(true);
    }
}
