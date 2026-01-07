using HC.Core.Application;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Domain.Orders;

namespace HC.LIS.Modules.TestOrders.Application.Orders.PlaceExamInProgress;

internal class PlaceExamInProgressCommandHandler(
    IOrderRepository orderRepository
) : ICommandHandler<PlaceExamInProgressCommand>
{
    private readonly IOrderRepository _orderRepository = orderRepository;
    public async Task Handle(
        PlaceExamInProgressCommand command,
        CancellationToken cancellationToken
    )
    {
        Order? order = await _orderRepository.GetByIdAsync(new OrderId(command.OrderId)).ConfigureAwait(false) ??
        throw new InvalidCommandException("Order must exist to place exam in progress");
        order.PlaceExamInProgress(new OrderItemId(command.ItemId), command.PlacedAt);
    }
}
