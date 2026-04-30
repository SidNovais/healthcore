using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HC.Core.Domain;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.SampleCollection.Application.Collections.GetSamplesByCollectionRequestId;
using HC.LIS.Modules.SampleCollection.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Orders.AcceptExam;
using HC.LIS.Modules.TestOrders.Application.Orders.CreateOrder;
using HC.LIS.Modules.TestOrders.Application.Orders.RequestExam;
using HC.LIS.Tests.IntegrationEvents.Probes;

namespace HC.LIS.Tests.IntegrationEvents.TestOrders;

public class OrderItemAcceptedFlowTests : TestBase
{
    [Fact]
    public async Task SingleExamAcceptedCreatesCollectionRequest()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();

        await TestOrdersModule.ExecuteCommandAsync(new CreateOrderCommand(
            orderId, patientId, ExecutionContext.UserId, "Routine", SystemClock.Now));

        await TestOrdersModule.ExecuteCommandAsync(new RequestExamCommand(
            orderId, orderItemId, "HGB", "BLOOD", "Whole Blood",
            "EDTA Tube", "EDTA", "Centrifuge", "Room Temperature", SystemClock.Now));

        // Act
        await TestOrdersModule.ExecuteCommandAsync(
            new AcceptExamCommand(orderId, orderItemId, SystemClock.Now));

        // Assert — SampleCollection receives OrderItemAcceptedIntegrationEvent
        //          and creates a CollectionRequest for the patient (async via Quartz)
        await IntegrationTestAssert.AssertEventually(
            new GetCollectionRequestFromSampleCollectionProbe(patientId, SampleCollectionModule),
            timeoutMs: 15_000);
    }

    [Fact]
    public async Task MultipleExamsAcceptedAddsExamsToSameCollectionRequest()
    {
        // Arrange — place order, request two exams, accept first
        var patientId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var orderItemId1 = Guid.NewGuid();
        var orderItemId2 = Guid.NewGuid();

        await TestOrdersModule.ExecuteCommandAsync(new CreateOrderCommand(
            orderId, patientId, ExecutionContext.UserId, "Routine", SystemClock.Now));

        await TestOrdersModule.ExecuteCommandAsync(new RequestExamCommand(
            orderId, orderItemId1, "HGB", "BLOOD", "Whole Blood",
            "EDTA Tube", "EDTA", "Centrifuge", "Room Temperature", SystemClock.Now));

        await TestOrdersModule.ExecuteCommandAsync(new RequestExamCommand(
            orderId, orderItemId2, "PLT", "BLOOD", "Whole Blood",
            "EDTA Tube", "EDTA", "Centrifuge", "Room Temperature", SystemClock.Now));

        await TestOrdersModule.ExecuteCommandAsync(
            new AcceptExamCommand(orderId, orderItemId1, SystemClock.Now));

        // Wait for the first CollectionRequest and capture its ID
        var crProbe = new GetCollectionRequestFromSampleCollectionProbe(patientId, SampleCollectionModule);
        await IntegrationTestAssert.AssertEventually(crProbe, timeoutMs: 15_000);
        var cr = await crProbe.GetSampleAsync();
        var collectionRequestId = cr!.CollectionRequestId;

        // Act — accept second exam; handler enqueues AddExamToCollectionForOrderCommand
        await TestOrdersModule.ExecuteCommandAsync(
            new AcceptExamCommand(orderId, orderItemId2, SystemClock.Now));

        // Assert — same CollectionRequest now has 2 samples (async via Quartz)
        await IntegrationTestAssert.AssertEventually(
            new TwoSamplesInCollectionRequestProbe(collectionRequestId, SampleCollectionModule),
            timeoutMs: 15_000);
    }

    private sealed class TwoSamplesInCollectionRequestProbe(
        Guid collectionRequestId,
        ISampleCollectionModule module) : IProbe<IReadOnlyCollection<SampleSummaryDto>>
    {
        public string DescribeFailureTo() =>
            $"Expected 2 samples in collection request {collectionRequestId}";

        public async Task<IReadOnlyCollection<SampleSummaryDto>?> GetSampleAsync()
        {
            var samples = await module.ExecuteQueryAsync(
                new GetSamplesByCollectionRequestIdQuery(collectionRequestId));
            return samples?.Count >= 2 ? samples : null;
        }

        public bool IsSatisfied(IReadOnlyCollection<SampleSummaryDto>? sample) => sample is not null;
    }
}
