using System;
using System.Threading;
using System.Threading.Tasks;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.LabAnalysis.Application.Physicians;
using HC.LIS.Modules.LabAnalysis.Application.Physicians.StorePhysicianSnapshot;
using HC.LIS.Modules.TestOrders.IntegrationEvents;
using NSubstitute;
using Xunit;

namespace HC.LIS.Modules.LabAnalysis.UnitTests.Physicians;

public class StorePhysicianSnapshotCommandHandlerTests
{
    private static readonly Guid PhysicianId = Guid.Parse("019b664c-52a4-7f37-a794-000000000101");
    private static readonly DateTime RegisteredAt = new(2026, 8, 5, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task HandlerStoresSnapshotWithAllFields()
    {
        IPhysicianSnapshotRepository repo = Substitute.For<IPhysicianSnapshotRepository>();
        var handler = new StorePhysicianSnapshotByPhysicianIdCommandHandler(repo);
        var command = new StorePhysicianSnapshotByPhysicianIdCommand(
            Guid.CreateVersion7(),
            PhysicianId,
            "Ana Lima",
            "CRM-12345",
            RegisteredAt
        );

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        await repo.Received(1).StoreAsync(
            PhysicianId,
            "Ana Lima",
            "CRM-12345",
            RegisteredAt
        ).ConfigureAwait(true);
    }

    [Fact]
    public async Task HandlerStoresSnapshotWithoutLicenceNumber()
    {
        IPhysicianSnapshotRepository repo = Substitute.For<IPhysicianSnapshotRepository>();
        var handler = new StorePhysicianSnapshotByPhysicianIdCommandHandler(repo);
        var command = new StorePhysicianSnapshotByPhysicianIdCommand(
            Guid.CreateVersion7(),
            PhysicianId,
            "Ana Lima",
            null,
            RegisteredAt
        );

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        await repo.Received(1).StoreAsync(
            PhysicianId,
            "Ana Lima",
            null,
            RegisteredAt
        ).ConfigureAwait(true);
    }

    [Fact]
    public async Task IntegrationEventHandlerEnqueuesStoreCommand()
    {
        ICommandsScheduler scheduler = Substitute.For<ICommandsScheduler>();
        var handler = new PhysicianRegisteredIntegrationEventNotificationHandler(scheduler);
        var notification = new PhysicianRegisteredIntegrationEvent(
            Guid.CreateVersion7(),
            RegisteredAt,
            PhysicianId,
            "Ana Lima",
            "CRM-12345",
            RegisteredAt
        );

        await handler.Handle(notification, CancellationToken.None).ConfigureAwait(true);

        await scheduler.Received(1)
            .EnqueueAsync(Arg.Is<StorePhysicianSnapshotByPhysicianIdCommand>(c =>
                c.PhysicianId == PhysicianId &&
                c.FullName == "Ana Lima" &&
                c.LicenceNumber == "CRM-12345" &&
                c.RegisteredAt == RegisteredAt
            )).ConfigureAwait(true);
    }
}
