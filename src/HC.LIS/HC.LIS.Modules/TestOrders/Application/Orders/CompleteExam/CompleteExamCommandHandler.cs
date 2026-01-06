using HC.Core.Application;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Domain.Orders;

namespace HC.LIS.Modules.TestOrders.Application.Orders.CompleteExam;

internal class CompleteExamCommandHandler(
    IOrderRepository orderRepository
) : ICommandHandler<CompleteExamCommand>
{
    private readonly IOrderRepository _orderRepository = orderRepository;
    public async Task Handle(
        CompleteExamCommand command,
        CancellationToken cancellationToken
    )
    {
        Order? order = await _orderRepository.GetByIdAsync(new OrderId(command.OrderId)).ConfigureAwait(false) ??
        throw new InvalidCommandException("Order must exist to complete exam");
        order.CompleteExam(new OrderItemId(command.ItemId), command.CompletedAt);
    }
}
