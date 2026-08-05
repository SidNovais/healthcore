using System;
using System.Threading;
using System.Threading.Tasks;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.LabAnalysis.Application.Orders;
using HC.LIS.Modules.LabAnalysis.Application.Orders.StoreOrderPhysician;
using HC.LIS.Modules.TestOrders.IntegrationEvents;
using NSubstitute;
using Xunit;

namespace HC.LIS.Modules.LabAnalysis.UnitTests.Orders;

public class StoreOrderPhysicianCommandHandlerTests
{
    private static readonly Guid OrderId = Guid.Parse("019b664c-52a4-7f37-a794-000000000201");
    private static readonly Guid PatientId = Guid.Parse("019b664c-52a4-7f37-a794-000000000202");
    private static readonly Guid PhysicianId = Guid.Parse("019b664c-52a4-7f37-a794-000000000101");
    private static readonly DateTime RequestedAt = new(2026, 8, 5, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task HandlerStoresTheOrderToPhysicianMapping()
    {
        IOrderPhysicianRepository repo = Substitute.For<IOrderPhysicianRepository>();
        var handler = new StoreOrderPhysicianByOrderIdCommandHandler(repo);
        var command = new StoreOrderPhysicianByOrderIdCommand(
            Guid.CreateVersion7(),
            OrderId,
            PhysicianId,
            RequestedAt
        );

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        await repo.Received(1).StoreAsync(OrderId, PhysicianId, RequestedAt).ConfigureAwait(true);
    }

    [Fact]
    public async Task IntegrationEventHandlerEnqueuesStoreCommand()
    {
        ICommandsScheduler scheduler = Substitute.For<ICommandsScheduler>();
        var handler = new OrderCreatedIntegrationEventNotificationHandler(scheduler);
        var notification = new OrderCreatedIntegrationEvent(
            Guid.CreateVersion7(),
            RequestedAt,
            OrderId,
            PatientId,
            PhysicianId,
            "Routine",
            RequestedAt
        );

        await handler.Handle(notification, CancellationToken.None).ConfigureAwait(true);

        await scheduler.Received(1)
            .EnqueueAsync(Arg.Is<StoreOrderPhysicianByOrderIdCommand>(c =>
                c.OrderId == OrderId &&
                c.PhysicianId == PhysicianId &&
                c.RequestedAt == RequestedAt
            )).ConfigureAwait(true);
    }
}
