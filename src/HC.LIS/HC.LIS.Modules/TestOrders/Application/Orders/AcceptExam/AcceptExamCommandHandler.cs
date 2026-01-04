using HC.Core.Application;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Domain.Orders;

namespace HC.LIS.Modules.TestOrders.Application.Orders.AcceptExam;

internal class AcceptExamCommandHandler(
    IOrderRepository orderRepository
) : ICommandHandler<AcceptExamCommand>
{
    private readonly IOrderRepository _orderRepository = orderRepository;
    public async Task Handle(
        AcceptExamCommand command,
        CancellationToken cancellationToken
    )
    {
        Order? order = await _orderRepository.GetByIdAsync(new OrderId(command.OrderId)).ConfigureAwait(false) ??
        throw new InvalidCommandException("Order must exist to accept exam");
        order.AcceptExam(new OrderItemId(command.ItemId), command.AcceptedAt);
    }
}
