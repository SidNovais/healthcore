using System.Threading.Tasks;
using HC.Core.Domain;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.CompleteWorklistItem;
using HC.LIS.Tests.IntegrationEvents.Probes;

namespace HC.LIS.Tests.IntegrationEvents.LabAnalysis;

public class WorklistItemCompletedFlowTests : TestBase
{
    [Fact]
    public async Task WorklistItemCompletedCompletesExamInTestOrders()
    {
        // Arrange + Act
        var (worklistItemId, orderItemId) = await SetupWorklistItemWithResultAsync("BC-P6-001", "HGB");
        await LabAnalysisModule.ExecuteCommandAsync(
            new CompleteWorklistItemCommand(worklistItemId, SystemClock.Now));

        // Assert
        await IntegrationTestAssert.AssertEventually(
            new GetExamCompletedFromTestOrdersProbe(orderItemId, TestOrdersModule),
            timeoutMs: 15_000);
    }
}
