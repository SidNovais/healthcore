using System;
using System.Threading;
using System.Threading.Tasks;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.LabAnalysis.Application.Physicians;
using HC.LIS.Modules.LabAnalysis.Application.Physicians.DeactivatePhysicianSnapshot;
using HC.LIS.Modules.TestOrders.IntegrationEvents;
using NSubstitute;
using Xunit;

namespace HC.LIS.Modules.LabAnalysis.UnitTests.Physicians;

public class DeactivatePhysicianSnapshotCommandHandlerTests
{
    private static readonly Guid PhysicianId = Guid.Parse("019b664c-52a4-7f37-a794-000000000101");
    private static readonly DateTime DeactivatedAt = new(2026, 8, 7, 14, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task HandlerDeactivatesSnapshot()
    {
        IPhysicianSnapshotRepository repo = Substitute.For<IPhysicianSnapshotRepository>();
        var handler = new DeactivatePhysicianSnapshotByPhysicianIdCommandHandler(repo);
        var command = new DeactivatePhysicianSnapshotByPhysicianIdCommand(
            Guid.CreateVersion7(),
            PhysicianId,
            DeactivatedAt
        );

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        await repo.Received(1).DeactivateAsync(PhysicianId, DeactivatedAt).ConfigureAwait(true);
    }

    [Fact]
    public async Task IntegrationEventHandlerEnqueuesDeactivateCommand()
    {
        ICommandsScheduler scheduler = Substitute.For<ICommandsScheduler>();
        var handler = new PhysicianDeactivatedIntegrationEventNotificationHandler(scheduler);
        var notification = new PhysicianDeactivatedIntegrationEvent(
            Guid.CreateVersion7(),
            DeactivatedAt,
            PhysicianId,
            DeactivatedAt
        );

        await handler.Handle(notification, CancellationToken.None).ConfigureAwait(true);

        await scheduler.Received(1)
            .EnqueueAsync(Arg.Is<DeactivatePhysicianSnapshotByPhysicianIdCommand>(c =>
                c.PhysicianId == PhysicianId &&
                c.DeactivatedAt == DeactivatedAt
            )).ConfigureAwait(true);
    }
}
