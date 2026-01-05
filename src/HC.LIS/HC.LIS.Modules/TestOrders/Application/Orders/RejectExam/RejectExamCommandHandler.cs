using HC.Core.Application;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Domain.Orders;

namespace HC.LIS.Modules.TestOrders.Application.Orders.RejectExam;

internal class RejectExamCommandHandler(
    IOrderRepository orderRepository
) : ICommandHandler<RejectExamCommand>
{
    private readonly IOrderRepository _orderRepository = orderRepository;
    public async Task Handle(
        RejectExamCommand command,
        CancellationToken cancellationToken
    )
    {
        Order? order = await _orderRepository.GetByIdAsync(new OrderId(command.OrderId)).ConfigureAwait(false) ??
        throw new InvalidCommandException("Order must exist to reject exam");
        order.RejectExam(new OrderItemId(command.ItemId), command.Reason, command.RejectedAt);
    }
}
