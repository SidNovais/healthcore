using System.Threading.Tasks;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.ForwardRawResult;
using HC.LIS.Tests.IntegrationEvents.Probes;

namespace HC.LIS.Tests.IntegrationEvents.Analyzer;

public class ExamResultReceivedFlowTests : TestBase
{
    [Fact]
    public async Task ExamResultReceivedRecordsAnalysisResultInLabAnalysis()
    {
        // Arrange + Act
        var (_, _, _, barcode, worklistItemId) = await SetupExamResultReadyAsync("HGB");
        await AnalyzerModule.ExecuteCommandAsync(new ForwardRawResultCommand(BuildOruR01(barcode, "HGB")));

        // Assert
        await IntegrationTestAssert.AssertEventually(
            new GetAnalysisResultFromLabAnalysisProbe(worklistItemId, LabAnalysisModule),
            timeoutMs: 15_000);
    }
}
