using System.Data;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.TestOrders.Application.Configuration.Queries;

namespace HC.LIS.Modules.TestOrders.Application.Orders.GetOrdersList;

internal class GetOrdersListQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory
) : IQueryHandler<GetOrdersListQuery, IReadOnlyCollection<OrderListItemDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<IReadOnlyCollection<OrderListItemDto>> Handle(
        GetOrdersListQuery query,
        CancellationToken cancellationToken
    )
    {
        string sql = @$"SELECT
            ""OrderDetails"".""Id"" AS ""{nameof(OrderListItemDto.OrderId)}"",
            ""OrderDetails"".""PatientId"" AS ""{nameof(OrderListItemDto.PatientId)}"",
            ""OrderDetails"".""RequestedBy"" AS ""{nameof(OrderListItemDto.RequestedBy)}"",
            ""OrderDetails"".""Priority"" AS ""{nameof(OrderListItemDto.OrderPriority)}"",
            ""OrderDetails"".""RequestedAt"" AS ""{nameof(OrderListItemDto.RequestedAt)}"",
            COUNT(""OrderItemDetails"".""Id"")::int AS ""{nameof(OrderListItemDto.ItemCount)}""
            FROM ""test_orders"".""OrderDetails"" AS ""OrderDetails""
            LEFT JOIN ""test_orders"".""OrderItemDetails"" AS ""OrderItemDetails""
                ON ""OrderDetails"".""Id"" = ""OrderItemDetails"".""OrderId""
            GROUP BY
                ""OrderDetails"".""Id"",
                ""OrderDetails"".""PatientId"",
                ""OrderDetails"".""RequestedBy"",
                ""OrderDetails"".""Priority"",
                ""OrderDetails"".""RequestedAt""
            ORDER BY ""OrderDetails"".""RequestedAt"" DESC";

        IDbConnection? connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to get orders list");

        IEnumerable<OrderListItemDto> results = await connection
            .QueryAsync<OrderListItemDto>(sql)
            .ConfigureAwait(false);

        return results.ToList().AsReadOnly();
    }
}
