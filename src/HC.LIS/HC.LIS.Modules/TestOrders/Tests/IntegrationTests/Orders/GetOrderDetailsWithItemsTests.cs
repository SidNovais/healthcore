using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using HC.Core.Domain;
using HC.LIS.Modules.TestOrders.Application.Orders.GetOrderDetails;
using HC.LIS.Modules.TestOrders.Application.Orders.RequestExam;

namespace HC.LIS.Modules.TestOrders.IntegrationTests.Orders;

public class GetOrderDetailsWithItemsTests : TestBase
{
    public GetOrderDetailsWithItemsTests() : base(Guid.CreateVersion7())
    {
        OrderFactory.CreateAsync(TestOrdersModule).GetAwaiter().GetResult();
        GetEventually(
            new GetOrderDetailFromTestOrdersProbe(OrderSampleData.OrderId, TestOrdersModule),
            15000
        ).GetAwaiter().GetResult();
        TestOrdersModule.ExecuteCommandAsync(
            new RequestExamCommand(
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
            )
        ).GetAwaiter().GetResult();
        GetEventually(
            new GetOrderItemDetailFromTestOrdersProbe(OrderSampleData.OrderItemId, TestOrdersModule),
            15000
        ).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task GetOrderDetailsIncludesItems()
    {
        OrderDetailsDto? result = await TestOrdersModule
            .ExecuteQueryAsync(new GetOrderDetailsQuery(OrderSampleData.OrderId))
            .ConfigureAwait(true);

        result.Should().NotBeNull();
        result!.OrderId.Should().Be(OrderSampleData.OrderId);
        result.Items.Should().HaveCount(1);

        var item = result.Items.First();
        item.OrderItemId.Should().Be(OrderSampleData.OrderItemId);
        item.SpecimenMnemonic.Should().Be(OrderSampleData.SpecimenMnemonic);
        item.Status.Should().Be("Requested");
    }
}
