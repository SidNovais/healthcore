using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using HC.LIS.Modules.LabAnalysis.IntegrationEvents;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Application.Orders.CompleteExam;
using HC.LIS.Modules.TestOrders.UnitTests.Orders;
using NSubstitute;

namespace HC.Lis.Modules.TestOrders.UnitTests.Orders;

public class WorklistItemCompletedIntegrationEventNotificationHandlerTests
{
    [Fact]
    public async Task HandleEnqueuesCompleteExamByExamIdCommand()
    {
        var scheduler = Substitute.For<ICommandsScheduler>();
        var handler = new WorklistItemCompletedIntegrationEventNotificationHandler(scheduler);
        var occurredAt = DateTime.UtcNow;
        var notification = new WorklistItemCompletedIntegrationEvent(
            Guid.CreateVersion7(),
            occurredAt,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            OrderSampleData.ExamMnemonic,
            "Complete",
            OrderSampleData.OrderId,
            OrderSampleData.OrderItemId
        );

        await handler.Handle(notification, CancellationToken.None).ConfigureAwait(true);

        await scheduler.Received(1).EnqueueAsync(
            Arg.Is<CompleteExamByExamIdCommand>(cmd =>
                cmd.OrderId == OrderSampleData.OrderId &&
                cmd.OrderItemId == OrderSampleData.OrderItemId &&
                cmd.CompletedAt == occurredAt)).ConfigureAwait(true);
    }
}
