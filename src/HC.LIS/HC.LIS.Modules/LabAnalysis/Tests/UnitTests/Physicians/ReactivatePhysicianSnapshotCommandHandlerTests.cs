using System;
using System.Threading;
using System.Threading.Tasks;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.LabAnalysis.Application.Physicians;
using HC.LIS.Modules.LabAnalysis.Application.Physicians.ReactivatePhysicianSnapshot;
using HC.LIS.Modules.TestOrders.IntegrationEvents;
using NSubstitute;
using Xunit;

namespace HC.LIS.Modules.LabAnalysis.UnitTests.Physicians;

public class ReactivatePhysicianSnapshotCommandHandlerTests
{
    private static readonly Guid PhysicianId = Guid.Parse("019b664c-52a4-7f37-a794-000000000101");
    private static readonly DateTime ReactivatedAt = new(2026, 8, 8, 8, 15, 0, DateTimeKind.Utc);

    [Fact]
    public async Task HandlerReactivatesSnapshot()
    {
        IPhysicianSnapshotRepository repo = Substitute.For<IPhysicianSnapshotRepository>();
        var handler = new ReactivatePhysicianSnapshotByPhysicianIdCommandHandler(repo);
        var command = new ReactivatePhysicianSnapshotByPhysicianIdCommand(
            Guid.CreateVersion7(),
            PhysicianId,
            ReactivatedAt
        );

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        await repo.Received(1).ReactivateAsync(PhysicianId, ReactivatedAt).ConfigureAwait(true);
    }

    [Fact]
    public async Task IntegrationEventHandlerEnqueuesReactivateCommand()
    {
        ICommandsScheduler scheduler = Substitute.For<ICommandsScheduler>();
        var handler = new PhysicianReactivatedIntegrationEventNotificationHandler(scheduler);
        var notification = new PhysicianReactivatedIntegrationEvent(
            Guid.CreateVersion7(),
            ReactivatedAt,
            PhysicianId,
            ReactivatedAt
        );

        await handler.Handle(notification, CancellationToken.None).ConfigureAwait(true);

        await scheduler.Received(1)
            .EnqueueAsync(Arg.Is<ReactivatePhysicianSnapshotByPhysicianIdCommand>(c =>
                c.PhysicianId == PhysicianId &&
                c.ReactivatedAt == ReactivatedAt
            )).ConfigureAwait(true);
    }
}
