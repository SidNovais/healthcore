using System;
using System.Threading.Tasks;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemDetails;

namespace HC.LIS.Tests.IntegrationEvents.Probes;

public sealed class GetAnalysisResultFromLabAnalysisProbe(
    Guid worklistItemId,
    ILabAnalysisModule module
) : IProbe<WorklistItemDetailsDto>
{
    public string DescribeFailureTo() =>
        $"WorklistItem {worklistItemId} did not reach ResultReceived status in LabAnalysis";

    public async Task<WorklistItemDetailsDto?> GetSampleAsync() =>
        await module
            .ExecuteQueryAsync(new GetWorklistItemDetailsQuery(worklistItemId))
            .ConfigureAwait(false);

    public bool IsSatisfied(WorklistItemDetailsDto? sample) =>
        sample?.Status == "ResultReceived";
}
