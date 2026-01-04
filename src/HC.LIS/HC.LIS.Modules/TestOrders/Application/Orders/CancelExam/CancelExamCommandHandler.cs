using HC.Core.Application;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Domain.Orders;

namespace HC.LIS.Modules.TestOrders.Application.Orders.CancelExam;

internal class CancelExamCommandHandler(
    IOrderRepository orderRepository
) : ICommandHandler<CancelExamCommand>
{
    private readonly IOrderRepository _orderRepository = orderRepository;
    public async Task Handle(
        CancelExamCommand command,
        CancellationToken cancellationToken
    )
    {
        Order? order = await _orderRepository.GetByIdAsync(new OrderId(command.OrderId)).ConfigureAwait(false) ??
        throw new InvalidCommandException("Order must exist to cancel exam");
        order.CancelExam(new OrderItemId(command.ItemId), command.CanceledAt);
    }
}
