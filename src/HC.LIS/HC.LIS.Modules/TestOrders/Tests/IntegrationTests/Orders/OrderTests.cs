using System;
using FluentAssertions;
using HC.Core.Domain;
using HC.LIS.Modules.TestOrders.Application.Orders.AcceptExam;
using HC.LIS.Modules.TestOrders.Application.Orders.CancelExam;
using HC.LIS.Modules.TestOrders.Application.Orders.GetOrderDetails;
using HC.LIS.Modules.TestOrders.Application.Orders.GetOrderItemDetails;
using HC.LIS.Modules.TestOrders.Application.Orders.RequestExam;

namespace HC.LIS.Modules.TestOrders.IntegrationTests.Orders;

public class OrderTests : TestBase
{
    private OrderDetailsDto? orderDetail;
    public OrderTests() : base(Guid.CreateVersion7())
    {
        OrderFactory.CreateAsync(TestOrdersModule).GetAwaiter().GetResult();
        orderDetail = GetEventually(
            new GetOrderDetailFromTestOrdersProbe(
                OrderSampleData.OrderId,
                TestOrdersModule
            ),
            15000
        ).GetAwaiter().GetResult();
        TestOrdersModule.ExecuteCommandAsync(
          new RequestExamCommand(
            OrderSampleData.OrderId,
            OrderSampleData.OrderItemId,
            OrderSampleData.SpecimenMnemonic,
            OrderSampleData.MaterialType,
            OrderSampleData.ContainerType,
            OrderSampleData.Additive,
            OrderSampleData.ProcessingType,
            OrderSampleData.StorageCondition,
            SystemClock.Now
          )
        ).GetAwaiter().GetResult();
    }

    [Fact]
    public async void CreateOrderIsSuccessfully()
    {
        orderDetail?.OrderId.Should().Be(OrderSampleData.OrderId);
        orderDetail?.PatientId.Should().Be(OrderSampleData.PatientId);
        orderDetail?.RequestedBy.Should().Be(OrderSampleData.RequestedBy);
        orderDetail?.OrderPriority.Should().Be(OrderSampleData.OrderPriority);
    }

    [Fact]
    public async void RequestExamIsSuccessfully()
    {
        OrderItemDetailsDto? orderItemDetails = await GetEventually(
            new GetOrderItemDetailFromTestOrdersProbe(
                OrderSampleData.OrderItemId,
                TestOrdersModule
            ),
            15000
        ).ConfigureAwait(true);
        orderItemDetails?.OrderId.Should().Be(OrderSampleData.OrderId);
        orderItemDetails?.OrderItemId.Should().Be(OrderSampleData.OrderItemId);
        orderItemDetails?.SpecimenMnemonic.Should().Be(OrderSampleData.SpecimenMnemonic);
        orderItemDetails?.MaterialType.Should().Be(OrderSampleData.MaterialType);
        orderItemDetails?.ContainerType.Should().Be(OrderSampleData.ContainerType);
        orderItemDetails?.Additive.Should().Be(OrderSampleData.Additive);
        orderItemDetails?.ProcessingType.Should().Be(OrderSampleData.ProcessingType);
        orderItemDetails?.StorageCondition.Should().Be(OrderSampleData.StorageCondition);
        orderItemDetails?.Status.Should().Be("Requested");
    }
    [Fact]
    public async void CancelExamIsSuccessfully()
    {
        await TestOrdersModule.ExecuteCommandAsync(
          new CancelExamCommand(
            OrderSampleData.OrderId,
            OrderSampleData.OrderItemId,
            SystemClock.Now
          )
        ).ConfigureAwait(true);

        OrderItemDetailsDto? orderItemDetails = await GetEventually(
            new GetOrderItemDetailFromTestOrdersProbe(
                OrderSampleData.OrderItemId,
                TestOrdersModule
            ),
            15000
        ).ConfigureAwait(true);
        orderItemDetails?.OrderId.Should().Be(OrderSampleData.OrderId);
        orderItemDetails?.OrderItemId.Should().Be(OrderSampleData.OrderItemId);
        orderItemDetails?.Status.Should().Be("Canceled");
        orderItemDetails?.CanceledAt.Should().NotBeNull();
    }

    [Fact]
    public async void AcceptExamIsSuccessfully()
    {
        await TestOrdersModule.ExecuteCommandAsync(
          new AcceptExamCommand(
            OrderSampleData.OrderId,
            OrderSampleData.OrderItemId,
            SystemClock.Now
          )
        ).ConfigureAwait(true);
        OrderItemDetailsDto? orderItemDetails = await GetEventually(
            new GetOrderItemDetailFromTestOrdersProbe(
                OrderSampleData.OrderItemId,
                TestOrdersModule
            ),
            15000
        ).ConfigureAwait(true);
        orderItemDetails?.OrderId.Should().Be(OrderSampleData.OrderId);
        orderItemDetails?.OrderItemId.Should().Be(OrderSampleData.OrderItemId);
        orderItemDetails?.Status.Should().Be("Accepted");
        orderItemDetails?.AcceptedAt.Should().NotBeNull();
    }
}
