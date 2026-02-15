using System.Data;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.TestOrders.Application.Configuration.Queries;

namespace HC.LIS.Modules.TestOrders.Application.Orders.GetOrderItemDetails;

internal class GetOrderItemDetailsQueryHandler(
  ISqlConnectionFactory sqlConnectionFactory
) : IQueryHandler<GetOrderItemDetailsQuery, OrderItemDetailsDto?>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<OrderItemDetailsDto?> Handle(
      GetOrderItemDetailsQuery query,
      CancellationToken cancellationToken
    )
    {
        string sql = @$"SELECT
        ""OrderItemDetails"".""Id"" AS ""{nameof(OrderItemDetailsDto.OrderItemId)}"",
        ""OrderItemDetails"".""OrderId"" AS ""{nameof(OrderItemDetailsDto.OrderId)}"",
        ""OrderItemDetails"".""SpecimenMnemonic"" AS ""{nameof(OrderItemDetailsDto.SpecimenMnemonic)}"",
        ""OrderItemDetails"".""MaterialType"" AS ""{nameof(OrderItemDetailsDto.MaterialType)}"",
        ""OrderItemDetails"".""ContainerType"" AS ""{nameof(OrderItemDetailsDto.ContainerType)}"",
        ""OrderItemDetails"".""Additive"" AS ""{nameof(OrderItemDetailsDto.Additive)}"",
        ""OrderItemDetails"".""ProcessingType"" AS ""{nameof(OrderItemDetailsDto.ProcessingType)}"",
        ""OrderItemDetails"".""StorageCondition"" AS ""{nameof(OrderItemDetailsDto.StorageCondition)}"",
        ""OrderItemDetails"".""Status"" AS ""{nameof(OrderItemDetailsDto.Status)}"",
        ""OrderItemDetails"".""RequestedAt"" AS ""{nameof(OrderItemDetailsDto.RequestedAt)}"",
        ""OrderItemDetails"".""CanceledAt"" AS ""{nameof(OrderItemDetailsDto.CanceledAt)}"",
        ""OrderItemDetails"".""OnHoldAt"" AS ""{nameof(OrderItemDetailsDto.OnHoldAt)}"",
        ""OrderItemDetails"".""AcceptedAt"" AS ""{nameof(OrderItemDetailsDto.AcceptedAt)}"",
        ""OrderItemDetails"".""RejectedAt"" AS ""{nameof(OrderItemDetailsDto.RejectedAt)}"",
        ""OrderItemDetails"".""InProgressAt"" AS ""{nameof(OrderItemDetailsDto.InProgressAt)}"",
        ""OrderItemDetails"".""PartiallyCompletedAt"" AS ""{nameof(OrderItemDetailsDto.PartiallyCompletedAt)}"",
        ""OrderItemDetails"".""CompletedAt"" AS ""{nameof(OrderItemDetailsDto.CompletedAt)}""
        FROM ""test_orders"".""OrderItemDetails"" AS ""OrderItemDetails""
        WHERE ""OrderItemDetails"".""Id"" = @OrderItemId"
        ;
        IDbConnection? connection = _sqlConnectionFactory.GetConnection()
        ?? throw new InvalidOperationException("Must exist connection to get order details");
        return await connection.QueryFirstOrDefaultAsync<OrderItemDetailsDto>(
            sql,
            new
            {
                query.OrderItemId
            }
        ).ConfigureAwait(false);
    }
}
