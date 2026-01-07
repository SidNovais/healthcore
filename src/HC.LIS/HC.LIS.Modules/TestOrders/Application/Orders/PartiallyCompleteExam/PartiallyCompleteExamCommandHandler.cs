using HC.Core.Application;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Domain.Orders;

namespace HC.LIS.Modules.TestOrders.Application.Orders.PartiallyCompleteExam;

internal class PartiallyCompleteExamCommandHandler(
    IOrderRepository orderRepository
) : ICommandHandler<PartiallyCompleteExamCommand>
{
    private readonly IOrderRepository _orderRepository = orderRepository;
    public async Task Handle(
        PartiallyCompleteExamCommand command,
        CancellationToken cancellationToken
    )
    {
        Order? order = await _orderRepository.GetByIdAsync(new OrderId(command.OrderId)).ConfigureAwait(false) ??
        throw new InvalidCommandException("Order must exist to partially complete exam");
        order.PartiallyCompleteExam(new OrderItemId(command.ItemId), command.PartiallyCompletedAt);
    }
}
