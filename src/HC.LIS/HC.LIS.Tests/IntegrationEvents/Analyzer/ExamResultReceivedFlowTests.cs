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
        var (_, _, _, _, worklistItemId) = await SetupExamResultReadyAsync("BC-P5-001", "HGB");
        await AnalyzerModule.ExecuteCommandAsync(new ForwardRawResultCommand(BuildOruR01("BC-P5-001", "HGB")));

        // Assert
        await IntegrationTestAssert.AssertEventually(
            new GetAnalysisResultFromLabAnalysisProbe(worklistItemId, LabAnalysisModule),
            timeoutMs: 15_000);
    }
}
