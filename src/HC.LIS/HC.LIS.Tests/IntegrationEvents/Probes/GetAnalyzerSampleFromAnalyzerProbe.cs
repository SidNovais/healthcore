using System.Threading.Tasks;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetSampleInfoByBarcode;
using HC.LIS.Modules.Analyzer.Application.Contracts;

namespace HC.LIS.Tests.IntegrationEvents.Probes;

public sealed class GetAnalyzerSampleFromAnalyzerProbe(
    string sampleBarcode,
    IAnalyzerModule module
) : IProbe<SampleInfoDto>
{
    public string DescribeFailureTo() =>
        $"AnalyzerSample for barcode '{sampleBarcode}' not found in Analyzer";

    public async Task<SampleInfoDto?> GetSampleAsync() =>
        await module
            .ExecuteQueryAsync(new GetSampleInfoByBarcodeQuery(sampleBarcode))
            .ConfigureAwait(false);

    public bool IsSatisfied(SampleInfoDto? sample) => sample is not null;
}
