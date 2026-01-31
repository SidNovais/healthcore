using System.Data;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.TestOrders.Application.Configuration.Queries;

namespace HC.LIS.Modules.TestOrders.Application.Orders.GetOrderDetails;

internal class GetOrderQueryDetailsHandler(
  ISqlConnectionFactory sqlConnectionFactory
) : IQueryHandler<GetOrderDetailsQuery, OrderDetailsDto?>
{
  private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

  public async Task<OrderDetailsDto?> Handle(
    GetOrderDetailsQuery query,
    CancellationToken cancellationToken
  )
  {
    string sql = @$"SELECT
      ""OrderDetails"".""Id"" AS ""{nameof(OrderDetailsDto.OrderId)}"",
      ""OrderDetails"".""PatientId"" AS ""{nameof(OrderDetailsDto.PatientId)}"",
      ""OrderDetails"".""RequestedBy"" AS ""{nameof(OrderDetailsDto.RequestedBy)}"",
      ""OrderDetails"".""Priority"" AS ""{nameof(OrderDetailsDto.OrderPriority)}"",
      ""OrderDetails"".""RequestedAt"" AS ""{nameof(OrderDetailsDto.RequestedAt)}""
      FROM ""test_orders"".""OrderDetails"" AS ""OrderDetails""
      WHERE ""OrderDetails"".""Id"" = @OrderId"
    ;
    IDbConnection? connection = _sqlConnectionFactory.GetConnection()
    ?? throw new InvalidOperationException("Must exist connection to get order details");
    return await connection.QuerySingleAsync<OrderDetailsDto>(
      sql,
      new
      {
        query.OrderId
      }
    ).ConfigureAwait(false);
  }
}
