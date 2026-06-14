using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FluentAssertions;
using Npgsql;
using HC.LIS.Modules.TestOrders.Application.Orders.GetOrdersList;

namespace HC.LIS.Modules.TestOrders.IntegrationTests.Orders;

public class GetOrdersListWithPatientInfoTests : TestBase
{
    private static readonly DateTime DateOfBirth = new(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc);

    public GetOrdersListWithPatientInfoTests() : base(Guid.CreateVersion7())
    {
        OrderFactory.CreateAsync(TestOrdersModule).GetAwaiter().GetResult();
        GetEventually(
            new GetOrderDetailFromTestOrdersProbe(OrderSampleData.OrderId, TestOrdersModule),
            15000
        ).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task OrdersListIncludesPatientNameWhenSnapshotExists()
    {
        using var connection = new NpgsqlConnection(ConnectionString);

        await connection.ExecuteAsync(
            """
            INSERT INTO "test_orders"."PatientSnapshotDetails"
                ("Id", "FullName", "DateOfBirth", "Gender", "Status", "RegisteredAt")
            VALUES
                (@Id, @FullName, @DateOfBirth, @Gender, @Status, @RegisteredAt)
            """,
            new
            {
                Id           = OrderSampleData.PatientId,
                FullName     = "Maria Silva",
                DateOfBirth,
                Gender       = "Female",
                Status       = "Active",
                RegisteredAt = DateTime.UtcNow
            }
        ).ConfigureAwait(true);

        IReadOnlyCollection<OrderListItemDto> result = await TestOrdersModule
            .ExecuteQueryAsync(new GetOrdersListQuery())
            .ConfigureAwait(true);

        OrderListItemDto item = result.Should().ContainSingle(x => x.OrderId == OrderSampleData.OrderId).Subject;
        item.PatientName.Should().Be("Maria Silva");
    }

    [Fact]
    public async Task OrdersListPatientNameIsNullWhenNoSnapshotExists()
    {
        IReadOnlyCollection<OrderListItemDto> result = await TestOrdersModule
            .ExecuteQueryAsync(new GetOrdersListQuery())
            .ConfigureAwait(true);

        OrderListItemDto item = result.Should().ContainSingle(x => x.OrderId == OrderSampleData.OrderId).Subject;
        item.PatientName.Should().BeNull();
    }
}
