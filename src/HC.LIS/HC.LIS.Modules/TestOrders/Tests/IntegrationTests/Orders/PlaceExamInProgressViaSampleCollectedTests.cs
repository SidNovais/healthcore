using System;
using System.Threading.Tasks;
using Dapper;
using FluentAssertions;
using Newtonsoft.Json;
using Npgsql;
using HC.Core.Domain;
using HC.Core.Infrastructure.Serialization;
using HC.LIS.Modules.SampleCollection.IntegrationEvents;
using HC.LIS.Modules.TestOrders.Application.Orders.AcceptExam;
using HC.LIS.Modules.TestOrders.Application.Orders.GetOrderItemDetails;
using HC.LIS.Modules.TestOrders.Application.Orders.RequestExam;

namespace HC.LIS.Modules.TestOrders.IntegrationTests.Orders;

public class PlaceExamInProgressViaSampleCollectedTests : TestBase
{
    public PlaceExamInProgressViaSampleCollectedTests() : base(Guid.CreateVersion7())
    {
        OrderFactory.CreateAsync(TestOrdersModule).GetAwaiter().GetResult();
        GetEventually(
            new GetOrderDetailFromTestOrdersProbe(OrderSampleData.OrderId, TestOrdersModule),
            15000
        ).GetAwaiter().GetResult();
        TestOrdersModule.ExecuteCommandAsync(new RequestExamCommand(
            OrderSampleData.OrderId,
            OrderSampleData.OrderItemId,
            OrderSampleData.ExamMnemonic,
            OrderSampleData.SpecimenMnemonic,
            OrderSampleData.MaterialType,
            OrderSampleData.ContainerType,
            OrderSampleData.Additive,
            OrderSampleData.ProcessingType,
            OrderSampleData.StorageCondition,
            SystemClock.Now
        )).GetAwaiter().GetResult();
        TestOrdersModule.ExecuteCommandAsync(new AcceptExamCommand(
            OrderSampleData.OrderId,
            OrderSampleData.OrderItemId,
            SystemClock.Now
        )).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task PlaceExamInProgressOnSampleCollectedIsSuccessful()
    {
        var integrationEvent = new SampleCollectedIntegrationEvent(
            Guid.CreateVersion7(),
            SystemClock.Now,
            OrderSampleData.OrderId,
            Guid.CreateVersion7(),
            OrderSampleData.PatientId,
            "SC-TEST-001",
            "John Doe",
            new DateTime(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            "M",
            isUrgent: false,
            [new ExamInfo(OrderSampleData.OrderItemId, OrderSampleData.ExamMnemonic)]
        );

        using (var connection = new NpgsqlConnection(ConnectionString))
        {
            string? type = integrationEvent.GetType().FullName;
            string data = JsonConvert.SerializeObject(integrationEvent, new JsonSerializerSettings
            {
                ContractResolver = new AllPropertiesContractResolver()
            });
            await connection.ExecuteScalarAsync(
                @"INSERT INTO ""test_orders"".""InboxMessages"" (""Id"", ""OccurredAt"", ""Type"", ""Data"") VALUES (@Id, @OccurredAt, @Type, @Data)",
                new { integrationEvent.Id, integrationEvent.OccurredAt, type, data }
            ).ConfigureAwait(true);
        }

        OrderItemDetailsDto? orderItemDetails = await GetEventually(
            new GetOrderItemInProgressFromTestOrdersProbe(OrderSampleData.OrderItemId, TestOrdersModule),
            15000
        ).ConfigureAwait(true);

        orderItemDetails.Should().NotBeNull();
        orderItemDetails!.Status.Should().Be("InProgress");
    }
}
