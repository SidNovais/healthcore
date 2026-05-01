using System.Linq;
using System.Threading.Tasks;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetSampleInfoByBarcode;
using HC.LIS.Modules.Analyzer.Application.Contracts;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemDetails;

namespace HC.LIS.Tests.IntegrationEvents.Probes;

public sealed class GetWorklistItemFromLabAnalysisProbe(
    string sampleBarcode,
    string examMnemonic,
    IAnalyzerModule analyzerModule,
    ILabAnalysisModule labAnalysisModule
) : IProbe<WorklistItemDetailsDto>
{
    public string DescribeFailureTo() =>
        $"WorklistItem for barcode '{sampleBarcode}' / exam '{examMnemonic}' not found in LabAnalysis";

    public async Task<WorklistItemDetailsDto?> GetSampleAsync()
    {
        var sampleInfo = await analyzerModule
            .ExecuteQueryAsync(new GetSampleInfoByBarcodeQuery(sampleBarcode))
            .ConfigureAwait(false);
        var exam = sampleInfo?.Exams.FirstOrDefault(e => e.ExamMnemonic == examMnemonic);
        if (exam?.WorklistItemId is not { } worklistItemId)
            return null;
        return await labAnalysisModule
            .ExecuteQueryAsync(new GetWorklistItemDetailsQuery(worklistItemId))
            .ConfigureAwait(false);
    }

    public bool IsSatisfied(WorklistItemDetailsDto? sample) => sample is not null;
}
