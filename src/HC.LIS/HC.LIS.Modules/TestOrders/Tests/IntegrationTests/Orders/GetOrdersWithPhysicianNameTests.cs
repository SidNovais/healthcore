using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using FluentAssertions;
using Npgsql;
using HC.Core.Domain;
using HC.LIS.Modules.TestOrders.Application.Orders.GetOrderDetails;
using HC.LIS.Modules.TestOrders.Application.Orders.GetOrdersList;

namespace HC.LIS.Modules.TestOrders.IntegrationTests.Orders;

/// <summary>
/// The physician name reaches the order read models through a LEFT JOIN on the registry, so orders
/// predating the registry (their "RequestedBy" holds a user id) must still come back, unnamed.
/// </summary>
public class GetOrdersWithPhysicianNameTests : TestBase
{
    private static readonly Guid LegacyOrderId = Guid.Parse("019b7e4a-1c33-7a20-8f61-4d2e9b7c1055");
    private static readonly Guid LegacyRequestedBy = Guid.Parse("019b7e4a-4f18-7c94-b3a7-08e6c1d5f722");

    public GetOrdersWithPhysicianNameTests() : base(Guid.CreateVersion7())
    {
        OrderFactory.CreateAsync(TestOrdersModule).GetAwaiter().GetResult();
        GetEventually(
            new GetOrderDetailFromTestOrdersProbe(OrderSampleData.OrderId, TestOrdersModule),
            15000
        ).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task OrdersListIncludesTheRegisteredPhysicianName()
    {
        IReadOnlyCollection<OrderListItemDto> result = await TestOrdersModule
            .ExecuteQueryAsync(new GetOrdersListQuery())
            .ConfigureAwait(true);

        OrderListItemDto item = result.Should().ContainSingle(x => x.OrderId == OrderSampleData.OrderId).Subject;
        item.RequestedBy.Should().Be(OrderSampleData.RequestedBy);
        item.RequestedByName.Should().Be(OrderSampleData.RequestedByFullName);
    }

    [Fact]
    public async Task OrdersListStillReturnsOrdersWhoseRequestedByIsNotInTheRegistry()
    {
        await InsertLegacyOrderAsync().ConfigureAwait(true);

        IReadOnlyCollection<OrderListItemDto> result = await TestOrdersModule
            .ExecuteQueryAsync(new GetOrdersListQuery())
            .ConfigureAwait(true);

        OrderListItemDto item = result.Should().ContainSingle(x => x.OrderId == LegacyOrderId).Subject;
        item.RequestedByName.Should().BeNull();
    }

    [Fact]
    public async Task OrderDetailsIncludesTheRegisteredPhysicianName()
    {
        OrderDetailsDto? result = await TestOrdersModule
            .ExecuteQueryAsync(new GetOrderDetailsQuery(OrderSampleData.OrderId))
            .ConfigureAwait(true);

        result.Should().NotBeNull();
        result!.RequestedByName.Should().Be(OrderSampleData.RequestedByFullName);
    }

    [Fact]
    public async Task OrderDetailsPhysicianNameIsNullWhenRequestedByIsNotInTheRegistry()
    {
        await InsertLegacyOrderAsync().ConfigureAwait(true);

        OrderDetailsDto? result = await TestOrdersModule
            .ExecuteQueryAsync(new GetOrderDetailsQuery(LegacyOrderId))
            .ConfigureAwait(true);

        result.Should().NotBeNull();
        result!.RequestedByName.Should().BeNull();
    }

    private async Task InsertLegacyOrderAsync()
    {
        using var connection = new NpgsqlConnection(ConnectionString);

        await connection.ExecuteAsync(
            """
            INSERT INTO "test_orders"."OrderDetails" ("Id", "PatientId", "Priority", "RequestedBy", "RequestedAt")
            VALUES (@Id, @PatientId, @Priority, @RequestedBy, @RequestedAt)
            """,
            new
            {
                Id = LegacyOrderId,
                PatientId = OrderSampleData.PatientId,
                Priority = OrderSampleData.OrderPriority,
                RequestedBy = LegacyRequestedBy,
                RequestedAt = SystemClock.Now
            }
        ).ConfigureAwait(true);
    }
}
