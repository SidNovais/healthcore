using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Domain.Orders;
using HC.LIS.Modules.TestOrders.Domain.Patients;
using HC.LIS.Modules.TestOrders.Domain.Physicians;

namespace HC.LIS.Modules.TestOrders.Application.Orders.CreateOrder;

internal class CreateOrderCommandHandler(
    IOrderRepository orderRepository
) : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository = orderRepository;
    public async Task<Guid> Handle(
        CreateOrderCommand command,
        CancellationToken cancellationToken
    )
    {
        Order order = Order.Create(
            command.OrderId,
            new PatientId(command.PatientId),
            new PhysicianId(command.RequestedBy),
            OrderPriority.Of(command.OrderPriority),
            command.RequestedAt
        );
        await _orderRepository.AddAsync(order).ConfigureAwait(false);
        return order.Id;
    }
}
