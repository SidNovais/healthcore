using HC.Core.Application;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Domain.Orders;

namespace HC.LIS.Modules.TestOrders.Application.Orders.RequestExam;

internal class RequestExamCommandHandler(
    IOrderRepository orderRepository
) : ICommandHandler<RequestExamCommand>
{
    private readonly IOrderRepository _orderRepository = orderRepository;
    public async Task Handle(
        RequestExamCommand command,
        CancellationToken cancellationToken
    )
    {
        Order? order = await _orderRepository.GetByIdAsync(new OrderId(command.OrderId)).ConfigureAwait(false) ??
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
