using System;
using System.Threading.Tasks;
using HC.Core.Domain;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.ForwardRawResult;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemDetails;
using HC.LIS.Tests.IntegrationEvents.Probes;

namespace HC.LIS.Tests.IntegrationEvents.LabAnalysis;

[Collection("IntegrationTests")]
public abstract class TestBase : HC.LIS.Tests.IntegrationEvents.Analyzer.TestBase
{
    protected async Task<(Guid worklistItemId, Guid orderItemId)>
        SetupWorklistItemWithResultAsync(string barcode, string examMnemonic)
    {
        var (_, orderItemId, _, _, worklistItemId) =
            await SetupExamResultReadyAsync(barcode, examMnemonic);

        await AnalyzerModule.ExecuteCommandAsync(
            new ForwardRawResultCommand(BuildOruR01(barcode, examMnemonic)));

        await IntegrationTestAssert.AssertEventually(
            new GetAnalysisResultFromLabAnalysisProbe(worklistItemId, LabAnalysisModule),
            timeoutMs: 15_000);

        await IntegrationTestAssert.AssertEventually(
            new WorklistItemInStatusProbe(worklistItemId, "ReportGenerated", LabAnalysisModule),
            timeoutMs: 30_000);

        return (worklistItemId, orderItemId);
    }

    private sealed class WorklistItemInStatusProbe(
        Guid worklistItemId,
        string expectedStatus,
        ILabAnalysisModule module) : IProbe<WorklistItemDetailsDto>
    {
        public string DescribeFailureTo() =>
            $"WorklistItem {worklistItemId} did not reach '{expectedStatus}' status";

        public async Task<WorklistItemDetailsDto?> GetSampleAsync() =>
            await module
                .ExecuteQueryAsync(new GetWorklistItemDetailsQuery(worklistItemId))
                .ConfigureAwait(false);

        public bool IsSatisfied(WorklistItemDetailsDto? sample) =>
            sample?.Status == expectedStatus;
    }
}
