using HC.Core.Application;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Domain.Orders;

namespace HC.LIS.Modules.TestOrders.Application.Orders.AcceptExam;

internal class AcceptExamCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<AcceptExamCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;
    public async Task Handle(
        AcceptExamCommand command,
        CancellationToken cancellationToken
    )
    {
        Order? order = await _aggregateStore.Load(new OrderId(command.OrderId)).ConfigureAwait(false) ??
        throw new InvalidCommandException("Order must exist to accept exam");
        order.AcceptExam(new OrderItemId(command.ItemId), command.AcceptedAt);
        _aggregateStore.AppendChanges(order);
    }
}
