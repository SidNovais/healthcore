using Dapper;
using HC.Core.Application.Projections;
using HC.Core.Domain;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.TestOrders.Domain.Orders.Events;

namespace HC.LIS.Modules.TestOrders.Application.Orders.GetOrderDetails;

internal class OrderDetailsProjector(
    ISqlConnectionFactory sqlConnectionFactory
) : ProjectorBase, IProjector
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;
    public async Task Project(IDomainEvent @event)
    {
        await When((dynamic)@event);
    }

    private async Task When(OrderCreatedDomainEvent orderCreated)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
          @$"INSERT INTO test_orders.""OrderDetails""
            (""Id"", ""PatientId"", ""Priority"" ,""RequestedBy"", ""RequestedAt"")
            VALUES (@OrderId, @PatientId, @OrderPriority, @RequestedBy, @RequestedAt)",
          new
          {
              orderCreated.OrderId,
              orderCreated.PatientId,
              orderCreated.OrderPriority,
              orderCreated.RequestedBy,
              orderCreated.RequestedAt
          }
        ).ConfigureAwait(false);
    }
}
