using System.Threading;
using System.Threading.Tasks;
using HC.Core.Application;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Domain.Orders;

namespace HC.LIS.Modules.TestOrders.Application.Orders.CompleteExam;

internal class CompleteExamByExamIdCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<CompleteExamByExamIdCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task Handle(
        CompleteExamByExamIdCommand command,
        CancellationToken cancellationToken
    )
    {
        Order? order = await _aggregateStore.Load(new OrderId(command.OrderId)).ConfigureAwait(false) ??
            throw new InvalidCommandException("Order must exist to complete exam");
        order.CompleteExam(new OrderItemId(command.OrderItemId), command.CompletedAt);
        _aggregateStore.AppendChanges(order);
    }
}
