using HC.Core.Application;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Domain.Orders;

namespace HC.LIS.Modules.TestOrders.Application.Orders.RejectExam;

internal class RejectExamCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<RejectExamCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;
    public async Task Handle(
        RejectExamCommand command,
        CancellationToken cancellationToken
    )
    {
        Order? order = await _aggregateStore.Load(new OrderId(command.OrderId)).ConfigureAwait(false) ??
        throw new InvalidCommandException("Order must exist to reject exam");
        order.RejectExam(new OrderItemId(command.ItemId), command.Reason, command.RejectedAt);
        _aggregateStore.AppendChanges(order);
    }
}
