using HC.Core.Application;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Domain.Orders;

namespace HC.LIS.Modules.TestOrders.Application.Orders.PlaceExamInProgress;

internal class PlaceExamInProgressCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<PlaceExamInProgressCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;
    public async Task Handle(
        PlaceExamInProgressCommand command,
        CancellationToken cancellationToken
    )
    {
        Order? order = await _aggregateStore.Load(new OrderId(command.OrderId)).ConfigureAwait(false) ??
        throw new InvalidCommandException("Order must exist to place exam in progress");
        order.PlaceExamInProgress(new OrderItemId(command.ItemId), command.PlacedAt);
        _aggregateStore.AppendChanges(order);
    }
}
