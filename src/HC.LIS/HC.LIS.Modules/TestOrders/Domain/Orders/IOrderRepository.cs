using System.Threading.Tasks;

namespace HC.LIS.Modules.TestOrders.Domain.Orders;

public interface IOrderRepository
{
    Task AddAsync(Order order);
    Task<Order?> GetByIdAsync(OrderId orderId);

}
