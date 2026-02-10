using HC.Core.Application;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Domain.Orders;

namespace HC.LIS.Modules.TestOrders.Application.Orders.RequestExam;

internal class RequestExamCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<RequestExamCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;
    public async Task Handle(
        RequestExamCommand command,
        CancellationToken cancellationToken
    )
    {
        Order? order = await _aggregateStore.Load(new OrderId(command.OrderId)).ConfigureAwait(false) ??
        throw new InvalidCommandException("Order must exist to request an test");
        order.RequestExam(
            command.ItemId,
            SpecimenRequirement.Of(
                command.SpecimenMnemonic,
                command.MaterialType,
                command.ContainerType,
                command.Additive,
                command.ProcessingType,
                command.StorageCondition
            ),
            command.RequestedAt
        );
    }
}
