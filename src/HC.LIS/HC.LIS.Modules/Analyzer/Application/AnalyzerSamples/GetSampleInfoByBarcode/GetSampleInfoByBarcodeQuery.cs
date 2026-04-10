using HC.LIS.Modules.Analyzer.Application.Contracts;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetSampleInfoByBarcode;

public class GetSampleInfoByBarcodeQuery(string sampleBarcode) : QueryBase<SampleInfoDto?>
{
    public string SampleBarcode { get; } = sampleBarcode;
}
