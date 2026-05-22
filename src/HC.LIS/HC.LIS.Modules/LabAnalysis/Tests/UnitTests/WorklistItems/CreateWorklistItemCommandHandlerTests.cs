using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.CreateWorklistItem;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;
using NSubstitute;
using Xunit;

namespace HC.LIS.Modules.LabAnalysis.UnitTests.WorklistItems;

public class CreateWorklistItemCommandHandlerTests
{
    [Fact]
    public async Task CreateWorklistItemIsIdempotentWhenAlreadyExists()
    {
        IAggregateStore aggregateStore = Substitute.For<IAggregateStore>();
        aggregateStore
            .Load<WorklistItem>(Arg.Any<WorklistItemId>())
            .Returns(Task.FromResult<WorklistItem?>(WorklistItemFactory.CreatePending()));

        var handler = new CreateWorklistItemCommandHandler(aggregateStore);
        var command = new CreateWorklistItemCommand(
            WorklistItemSampleData.WorklistItemId,
            WorklistItemSampleData.SampleId,
            WorklistItemSampleData.SampleBarcode,
            WorklistItemSampleData.ExamCode,
            WorklistItemSampleData.PatientId,
            WorklistItemSampleData.OrderId,
            WorklistItemSampleData.OrderItemId,
            WorklistItemSampleData.CreatedAt
        );

        Guid result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().Be(WorklistItemSampleData.WorklistItemId);
        aggregateStore.DidNotReceive().Start(Arg.Any<WorklistItem>());
    }
}
