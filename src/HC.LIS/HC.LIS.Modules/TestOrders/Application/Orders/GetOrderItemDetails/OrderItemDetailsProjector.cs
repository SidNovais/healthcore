using Dapper;
using HC.Core.Application.Projections;
using HC.Core.Domain;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.TestOrders.Domain.Orders;
using HC.LIS.Modules.TestOrders.Domain.Orders.Events;

namespace HC.LIS.Modules.TestOrders.Application.Orders.GetOrderItemDetails;

internal class OrderItemDetailsProjector(
    ISqlConnectionFactory sqlConnectionFactory
) : ProjectorBase, IProjector
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;
    public async Task Project(IDomainEvent @event)
    {
        await When((dynamic)@event);
    }

    private async Task When(OrderItemRequestedDomainEvent orderItemRequest)
    {
        string status = OrderItemStatus.Requested.Value;
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
          @$"INSERT INTO test_orders.""OrderItemDetails""
            (""Id"", ""OrderId"", ""SpecimenMnemonic"" ,""MaterialType"", ""ContainerType"", ""Additive"", ""ProcessingType"",
            ""StorageCondition"", ""Status"", ""RequestedAt"")
            VALUES (@OrderItemId, @OrderId, @SpecimenMnemonic, @MaterialType, @ContainerType, @Additive, @ProcessingType,
            @StorageCondition, @Status, @RequestedAt)",
          new
          {
              orderItemRequest.OrderItemId,
              orderItemRequest.OrderId,
              orderItemRequest.SpecimenMnemonic,
              orderItemRequest.MaterialType,
              orderItemRequest.ContainerType,
              orderItemRequest.Additive,
              orderItemRequest.ProcessingType,
              orderItemRequest.StorageCondition,
              Status = status,
              orderItemRequest.RequestedAt
          }
        ).ConfigureAwait(false);
    }
    private async Task When(OrderItemCanceledDomainEvent orderItemCanceled)
    {
        string status = OrderItemStatus.Canceled.Value;
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
        $@"UPDATE test_orders.""OrderItemDetails""
        SET ""Status"" = @Status,
        ""CanceledAt"" = @CanceledAt
        WHERE ""Id"" = @OrderItemId ",
        new
        {
            orderItemCanceled.OrderItemId,
            Status = status,
            orderItemCanceled.CanceledAt
        }).ConfigureAwait(false);
    }

    private async Task When(OrderItemAcceptedDomainEvent orderItemAccepted)
    {
        string status = OrderItemStatus.Accepted.Value;
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
        $@"UPDATE test_orders.""OrderItemDetails""
        SET ""Status"" = @Status,
        ""AcceptedAt"" = @AcceptedAt
        WHERE ""Id"" = @OrderItemId ",
        new
        {
            orderItemAccepted.OrderItemId,
            Status = status,
            orderItemAccepted.AcceptedAt
        }).ConfigureAwait(false);
    }

    private async Task When(OrderItemRejectedDomainEvent orderItemRejected)
    {
        string status = OrderItemStatus.Rejected.Value;
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
        $@"UPDATE test_orders.""OrderItemDetails""
        SET ""Status"" = @Status,
        ""ReasonForRejection"" = @Reason,
        ""RejectedAt"" = @RejectedAt
        WHERE ""Id"" = @OrderItemId ",
        new
        {
            orderItemRejected.OrderItemId,
            Status = status,
            orderItemRejected.Reason,
            orderItemRejected.RejectedAt
        }).ConfigureAwait(false);
    }
}
