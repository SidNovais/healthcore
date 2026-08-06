using System.Threading.Tasks;
using HC.LIS.Tests.IntegrationEvents.Probes;

namespace HC.LIS.Tests.IntegrationEvents.SampleCollection;

public class SampleCollectedFlowTests : TestBase
{
    [Fact]
    public async Task SampleCollectedPlacesExamInProgressInTestOrders()
    {
        var (_, orderItemId, _, _) = await SetupCollectedSampleAsync("HGB");

        await IntegrationTestAssert.AssertEventually(
            new GetExamInProgressFromTestOrdersProbe(orderItemId, TestOrdersModule),
            timeoutMs: 15_000);
    }

    [Fact]
    public async Task SampleCollectedCreatesAnalyzerSample()
    {
        var (_, _, _, barcode) = await SetupCollectedSampleAsync("HGB");

        await IntegrationTestAssert.AssertEventually(
            new GetAnalyzerSampleFromAnalyzerProbe(barcode, AnalyzerModule),
            timeoutMs: 15_000);
    }

    [Fact]
    public async Task SampleCollectedCreatesWorklistItemInLabAnalysis()
    {
        var (_, _, _, barcode) = await SetupCollectedSampleAsync("HGB");

        await IntegrationTestAssert.AssertEventually(
            new GetWorklistItemFromLabAnalysisProbe(barcode, "HGB", AnalyzerModule, LabAnalysisModule),
            timeoutMs: 25_000);
    }

    [Fact]
    public async Task SampleCollectedAssignsWorklistItemToAnalyzerExam()
    {
        var (_, _, _, barcode) = await SetupCollectedSampleAsync("HGB");

        await IntegrationTestAssert.AssertEventually(
            new GetWorklistItemAssignedFromAnalyzerProbe(barcode, "HGB", AnalyzerModule),
            timeoutMs: 25_000);
    }
}
