using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HC.Core.Domain;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.ForwardRawResult;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetSampleInfoByBarcode;
using HC.LIS.Modules.Analyzer.Application.Contracts;
using HC.LIS.Modules.Analyzer.Infrastructure;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.CompleteWorklistItem;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemDetails;
using HC.LIS.Modules.LabAnalysis.Infrastructure;
using HC.LIS.Modules.SampleCollection.Application.Collections.CallPatient;
using HC.LIS.Modules.SampleCollection.Application.Collections.GetSamplesByCollectionRequestId;
using HC.LIS.Modules.SampleCollection.Application.Collections.CreateBarcode;
using HC.LIS.Modules.SampleCollection.Application.Collections.MovePatientToWaiting;
using HC.LIS.Modules.SampleCollection.Application.Collections.RecordSampleCollection;
using HC.LIS.Modules.SampleCollection.Infrastructure;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Orders.AcceptExam;
using HC.LIS.Modules.TestOrders.Application.Orders.CreateOrder;
using HC.LIS.Modules.TestOrders.Application.Orders.RequestExam;
using HC.LIS.Modules.TestOrders.Infrastructure;
using HC.LIS.Tests.IntegrationEvents.Probes;

namespace HC.LIS.Tests.IntegrationEvents;

[Collection("IntegrationTests")]
public class FullWorkflowTests : TestBase
{
    private TestOrdersModule _testOrders = null!;
    private SampleCollectionModule _sampleCollection = null!;
    private AnalyzerModule _analyzer = null!;
    private LabAnalysisModule _labAnalysis = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _testOrders       = new TestOrdersModule();
        _sampleCollection = new SampleCollectionModule();
        _analyzer         = new AnalyzerModule();
        _labAnalysis      = new LabAnalysisModule();
    }

    [Fact]
    public async Task FullLaboratoryWorkflowFromOrderAcceptanceToCompletion()
    {
        const string barcode      = "BC-P7-001";
        const string examMnemonic = "HGB";
        var patientId   = Guid.NewGuid();
        var orderId     = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();

        // Step 1: TestOrders — place order, request exam, accept exam
        await _testOrders.ExecuteCommandAsync(new CreateOrderCommand(
            orderId, patientId, ExecutionContext.UserId, "Routine", SystemClock.Now));
        await _testOrders.ExecuteCommandAsync(new RequestExamCommand(
            orderId, orderItemId, examMnemonic, "BLOOD", "Whole Blood",
            "EDTA Tube", "EDTA", "Centrifuge", "Room Temperature", SystemClock.Now));
        await _testOrders.ExecuteCommandAsync(new AcceptExamCommand(orderId, orderItemId, SystemClock.Now));

        // Step 2: Wait for SampleCollection to receive OrderItemAcceptedIntegrationEvent
        var crProbe = new GetCollectionRequestFromSampleCollectionProbe(patientId, _sampleCollection);
        await IntegrationTestAssert.AssertEventually(crProbe, timeoutMs: 60_000);
        var collectionRequestId = (await crProbe.GetSampleAsync())!.CollectionRequestId;

        // Step 3: Drive SampleCollection state machine + record sample
        await _sampleCollection.ExecuteCommandAsync(new MovePatientToWaitingCommand(collectionRequestId, SystemClock.Now));
        await _sampleCollection.ExecuteCommandAsync(new CreateBarcodeCommand(collectionRequestId, "EDTA Tube", barcode, ExecutionContext.UserId, SystemClock.Now));
        await _sampleCollection.ExecuteCommandAsync(new CallPatientCommand(collectionRequestId, ExecutionContext.UserId, SystemClock.Now));

        var samples = await _sampleCollection.ExecuteQueryAsync(
            new GetSamplesByCollectionRequestIdQuery(collectionRequestId));
        var sampleId = samples!.Single().Id;

        await _sampleCollection.ExecuteCommandAsync(new RecordSampleCollectionCommand(
            collectionRequestId, sampleId, ExecutionContext.UserId,
            "Test Patient", new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc), "M", SystemClock.Now));

        // Step 4: Assert fan-out from SampleCollectedIntegrationEvent (all downstream effects)
        await IntegrationTestAssert.AssertEventually(
            new GetExamInProgressFromTestOrdersProbe(orderItemId, _testOrders), timeoutMs: 60_000);
        await IntegrationTestAssert.AssertEventually(
            new GetAnalyzerSampleFromAnalyzerProbe(barcode, _analyzer), timeoutMs: 60_000);
        // Two-hop: SampleCollected → WorklistItem created in LabAnalysis → assigned back to AnalyzerSample
        await IntegrationTestAssert.AssertEventually(
            new GetWorklistItemAssignedFromAnalyzerProbe(barcode, examMnemonic, _analyzer), timeoutMs: 60_000);

        // Retrieve worklistItemId from Analyzer facade (WorklistItemId is populated after assignment)
        var sampleInfo = await _analyzer.ExecuteQueryAsync(new GetSampleInfoByBarcodeQuery(barcode));
        var worklistItemId = sampleInfo!.Exams.Single(e => e.ExamMnemonic == examMnemonic).WorklistItemId!.Value;

        // Step 5: Analyzer — forward raw HL7 result
        await _analyzer.ExecuteCommandAsync(new ForwardRawResultCommand(BuildOruR01(barcode, examMnemonic)));

        // Step 6: Assert analysis result recorded + report auto-generated
        await IntegrationTestAssert.AssertEventually(
            new GetAnalysisResultFromLabAnalysisProbe(worklistItemId, _labAnalysis), timeoutMs: 60_000);
        await IntegrationTestAssert.AssertEventually(
            new ReportGeneratedProbe(worklistItemId, _labAnalysis), timeoutMs: 60_000);

        // Step 7: LabAnalysis — complete worklist item → fires WorklistItemCompletedIntegrationEvent
        await _labAnalysis.ExecuteCommandAsync(new CompleteWorklistItemCommand(worklistItemId, SystemClock.Now));

        // Step 8: Assert exam completed in TestOrders
        await IntegrationTestAssert.AssertEventually(
            new GetExamCompletedFromTestOrdersProbe(orderItemId, _testOrders), timeoutMs: 60_000);
    }

    private static byte[] BuildOruR01(string barcode, string examMnemonic)
    {
        string msg =
            $"MSH|^~\\&|Analyzer|Lab|||20260430000000||ORU^R01|1|P|2.5\r" +
            $"SPM|||{barcode}\r" +
            $"OBX|1|NM|{examMnemonic}^Description||5.0|mg/dL|3.5-7.0\r";
        return Encoding.UTF8.GetBytes(msg);
    }

    private sealed class ReportGeneratedProbe(
        Guid worklistItemId,
        ILabAnalysisModule module) : IProbe<WorklistItemDetailsDto>
    {
        public string DescribeFailureTo() =>
            $"WorklistItem {worklistItemId} did not reach 'ReportGenerated' status";

        public async Task<WorklistItemDetailsDto?> GetSampleAsync() =>
            await module
                .ExecuteQueryAsync(new GetWorklistItemDetailsQuery(worklistItemId))
                .ConfigureAwait(false);

        public bool IsSatisfied(WorklistItemDetailsDto? sample) =>
            sample?.Status == "ReportGenerated";
    }
}
