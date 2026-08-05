using System;
using System.Threading.Tasks;
using FluentAssertions;
using HC.Core.Domain;
using HC.LIS.Modules.TestOrders.Application.Orders.CreateOrder;
using HC.LIS.Modules.TestOrders.Application.Orders.GetOrderDetails;
using HC.LIS.Modules.TestOrders.Application.Physicians.DeactivatePhysician;
using HC.LIS.Modules.TestOrders.Domain.Orders.Rules;
using HC.LIS.Modules.TestOrders.IntegrationTests.Physicians;

namespace HC.LIS.Modules.TestOrders.IntegrationTests.Orders;

public class CreateOrderPhysicianEnforcementTests : TestBase
{
    public CreateOrderPhysicianEnforcementTests() : base(Guid.CreateVersion7()) { }

    [Fact]
    public async Task CreateOrderThrowsWhenPhysicianIsNotRegistered()
    {
        Func<Task> action = async () => await TestOrdersModule.ExecuteCommandAsync(
            new CreateOrderCommand(
                OrderSampleData.OrderId,
                OrderSampleData.PatientId,
                OrderSampleData.UnregisteredPhysicianId,
                OrderSampleData.OrderPriority,
                OrderSampleData.RequestedAt
            )
        ).ConfigureAwait(true);

        (await action.Should().ThrowAsync<BaseBusinessRuleException>().ConfigureAwait(true))
            .Which.Rule.Should().BeOfType<OrderMustReferenceRegisteredPhysicianRule>();
    }

    [Fact]
    public async Task CreateOrderThrowsWhenPhysicianIsInactive()
    {
        await OrderFactory.RegisterRequestingPhysicianAsync(TestOrdersModule).ConfigureAwait(true);

        await TestOrdersModule.ExecuteCommandAsync(
            new DeactivatePhysicianCommand(OrderSampleData.RequestedBy, SystemClock.Now)
        ).ConfigureAwait(true);
        await GetEventually(
            new GetPhysicianDetailsFromTestOrdersProbe(
                OrderSampleData.RequestedBy,
                TestOrdersModule,
                dto => dto?.Status == "Inactive"),
            15000
        ).ConfigureAwait(true);

        Func<Task> action = async () => await TestOrdersModule.ExecuteCommandAsync(
            new CreateOrderCommand(
                OrderSampleData.OrderId,
                OrderSampleData.PatientId,
                OrderSampleData.RequestedBy,
                OrderSampleData.OrderPriority,
                OrderSampleData.RequestedAt
            )
        ).ConfigureAwait(true);

        (await action.Should().ThrowAsync<BaseBusinessRuleException>().ConfigureAwait(true))
            .Which.Rule.Should().BeOfType<OrderMustReferenceActivePhysicianRule>();
    }

    [Fact]
    public async Task CreateOrderSucceedsWhenPhysicianIsRegisteredAndActive()
    {
        await OrderFactory.CreateAsync(TestOrdersModule).ConfigureAwait(true);

        OrderDetailsDto? orderDetail = await GetEventually(
            new GetOrderDetailFromTestOrdersProbe(
                OrderSampleData.OrderId,
                TestOrdersModule
            ),
            15000
        ).ConfigureAwait(true);

        orderDetail.Should().NotBeNull();
        orderDetail!.RequestedBy.Should().Be(OrderSampleData.RequestedBy);
    }
}
