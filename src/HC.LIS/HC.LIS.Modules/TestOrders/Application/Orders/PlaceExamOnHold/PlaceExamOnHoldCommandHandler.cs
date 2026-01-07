using HC.Core.Application;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Domain.Orders;

namespace HC.LIS.Modules.TestOrders.Application.Orders.PlaceExamOnHold;

internal class PlaceExamOnHoldCommandHandler(
    IOrderRepository orderRepository
) : ICommandHandler<PlaceExamOnHoldCommand>
{
    private readonly IOrderRepository _orderRepository = orderRepository;
    public async Task Handle(
        PlaceExamOnHoldCommand command,
        CancellationToken cancellationToken
    )
    {
        Order? order = await _orderRepository.GetByIdAsync(new OrderId(command.OrderId)).ConfigureAwait(false) ??
        throw new InvalidCommandException("Order must exist to place exam on hold");
        order.PlaceExamOnHold(new OrderItemId(command.ItemId), command.Reason, command.PlacedAt);
    }
}
