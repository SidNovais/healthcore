using System;
using System.Threading.Tasks;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemDetails;

namespace HC.LIS.Modules.LabAnalysis.IntegrationTests.WorklistItems;

public class GetWorklistItemDetailsFromLabAnalysisProbe(
    Guid expectedWorklistItemId,
    ILabAnalysisModule labAnalysisModule,
    Func<WorklistItemDetailsDto?, bool>? satisfiedWhen = null
) : IProbe<WorklistItemDetailsDto>
{
    private readonly Guid _expectedWorklistItemId = expectedWorklistItemId;
    private readonly ILabAnalysisModule _labAnalysisModule = labAnalysisModule;
    private readonly Func<WorklistItemDetailsDto?, bool> _satisfiedWhen = satisfiedWhen ?? (dto => dto is not null);

    public string DescribeFailureTo() =>
        $"WorklistItemDetails not found or unsatisfied for {_expectedWorklistItemId}";

    public async Task<WorklistItemDetailsDto?> GetSampleAsync()
    {
        return await _labAnalysisModule
            .ExecuteQueryAsync(new GetWorklistItemDetailsQuery(_expectedWorklistItemId))
            .ConfigureAwait(false);
    }

    public bool IsSatisfied(WorklistItemDetailsDto? sample) => _satisfiedWhen(sample);
}
