using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HC.Core.Domain;
using HC.LIS.Modules.TestOrders.Application.Orders.GetOrdersList;
using HC.LIS.Modules.TestOrders.Application.Orders.RequestExam;

namespace HC.LIS.Modules.TestOrders.IntegrationTests.Orders;

public class GetOrdersListTests : TestBase
{
    public GetOrdersListTests() : base(Guid.CreateVersion7())
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
    public async void GetOrdersListIsSuccessful()
    {
        IReadOnlyCollection<OrderListItemDto> result = await TestOrdersModule
            .ExecuteQueryAsync(new GetOrdersListQuery())
            .ConfigureAwait(true);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);

        OrderListItemDto item = result.First();
        item.OrderId.Should().Be(OrderSampleData.OrderId);
        item.PatientId.Should().Be(OrderSampleData.PatientId);
        item.RequestedBy.Should().Be(OrderSampleData.RequestedBy);
        item.OrderPriority.Should().Be(OrderSampleData.OrderPriority);
        item.ItemCount.Should().Be(1);
    }
}
