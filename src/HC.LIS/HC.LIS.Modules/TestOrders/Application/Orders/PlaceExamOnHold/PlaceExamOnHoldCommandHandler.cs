using HC.Core.Application;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Domain.Orders;

namespace HC.LIS.Modules.TestOrders.Application.Orders.PlaceExamOnHold;

internal class PlaceExamOnHoldCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<PlaceExamOnHoldCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;
    public async Task Handle(
        PlaceExamOnHoldCommand command,
        CancellationToken cancellationToken
    )
    {
        Order? order = await _aggregateStore.Load(new OrderId(command.OrderId)).ConfigureAwait(false) ??
        throw new InvalidCommandException("Order must exist to place exam on hold");
        order.PlaceExamOnHold(new OrderItemId(command.ItemId), command.Reason, command.PlacedAt);
        _aggregateStore.AppendChanges(order);
    }
}
