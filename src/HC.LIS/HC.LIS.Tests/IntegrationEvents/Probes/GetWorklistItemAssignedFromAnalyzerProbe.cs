using System.Linq;
using System.Threading.Tasks;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetSampleInfoByBarcode;
using HC.LIS.Modules.Analyzer.Application.Contracts;

namespace HC.LIS.Tests.IntegrationEvents.Probes;

public sealed class GetWorklistItemAssignedFromAnalyzerProbe(
    string sampleBarcode,
    string examMnemonic,
    IAnalyzerModule module
) : IProbe<SampleInfoDto>
{
    public string DescribeFailureTo() =>
        $"WorklistItemId not assigned to exam '{examMnemonic}' on analyzer sample '{sampleBarcode}'";

    public async Task<SampleInfoDto?> GetSampleAsync() =>
        await module
            .ExecuteQueryAsync(new GetSampleInfoByBarcodeQuery(sampleBarcode))
            .ConfigureAwait(false);

    public bool IsSatisfied(SampleInfoDto? sample) =>
        sample?.Exams.Any(e => e.ExamMnemonic == examMnemonic && e.WorklistItemId.HasValue) == true;
}
