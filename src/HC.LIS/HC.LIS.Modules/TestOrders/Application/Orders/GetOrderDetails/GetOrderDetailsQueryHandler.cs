using System.Data;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.TestOrders.Application.Configuration.Queries;
using HC.LIS.Modules.TestOrders.Application.Orders.GetOrderItemDetails;

namespace HC.LIS.Modules.TestOrders.Application.Orders.GetOrderDetails;

internal class GetOrderDetailsQueryHandler(
  ISqlConnectionFactory sqlConnectionFactory
) : IQueryHandler<GetOrderDetailsQuery, OrderDetailsDto?>
{
  private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

  public async Task<OrderDetailsDto?> Handle(
    GetOrderDetailsQuery query,
    CancellationToken cancellationToken
  )
  {
    const string sql = @"
      SELECT
        od.""Id"" AS ""OrderId"",
        od.""PatientId"",
        od.""RequestedBy"",
        od.""Priority"" AS ""OrderPriority"",
        od.""RequestedAt""
      FROM test_orders.""OrderDetails"" od
      WHERE od.""Id"" = @OrderId;

      SELECT
        oid.""Id"" AS ""OrderItemId"",
        oid.""OrderId"",
        oid.""SpecimenMnemonic"",
        oid.""MaterialType"",
        oid.""ContainerType"",
        oid.""Additive"",
        oid.""ProcessingType"",
        oid.""StorageCondition"",
        oid.""Status"",
        COALESCE(oid.""ReasonForRejection"", '') AS ""ReasonForRejection"",
        oid.""RequestedAt"",
        oid.""CanceledAt"",
        oid.""OnHoldAt"",
        oid.""AcceptedAt"",
        oid.""RejectedAt"",
        oid.""InProgressAt"",
        oid.""PartiallyCompletedAt"",
        oid.""CompletedAt""
      FROM test_orders.""OrderItemDetails"" oid
      WHERE oid.""OrderId"" = @OrderId";

    IDbConnection connection = _sqlConnectionFactory.GetConnection()
      ?? throw new InvalidOperationException("Must exist connection to get order details");
    using SqlMapper.GridReader multi = await connection
      .QueryMultipleAsync(sql, new { query.OrderId })
      .ConfigureAwait(false);
    OrderDetailsDto? order = await multi
      .ReadFirstOrDefaultAsync<OrderDetailsDto>()
      .ConfigureAwait(false);
    if (order is null) return null;
    order.Items = (await multi.ReadAsync<OrderItemDetailsDto>().ConfigureAwait(false))
      .ToList()
      .AsReadOnly();
    return order;
  }
}
