using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetSampleInfoByBarcode;

namespace HC.LIS.Modules.Analyzer.Application.Contracts;

public interface ISampleInfoPresenter
{
    string Format(SampleInfoDto sampleInfo);
}
