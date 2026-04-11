using HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples;

namespace HC.LIS.Modules.Analyzer.UnitTests.AnalyzerSamples;

internal static class AnalyzerSampleFactory
{
    public static AnalyzerSample Create()
    {
        return AnalyzerSample.Create(
            AnalyzerSampleSampleData.AnalyzerSampleId,
            AnalyzerSampleSampleData.SampleId,
            AnalyzerSampleSampleData.SampleBarcode,
            AnalyzerSampleSampleData.PatientInfo,
            [
                new ExamInfo(AnalyzerSampleSampleData.ExamId, AnalyzerSampleSampleData.ExamMnemonic),
                new ExamInfo(AnalyzerSampleSampleData.ExamId2, AnalyzerSampleSampleData.ExamMnemonic2)
            ],
            isUrgent: false,
            AnalyzerSampleSampleData.CreatedAt
        );
    }

    public static AnalyzerSample CreateWithInfoDispatched()
    {
        AnalyzerSample sut = Create();
        sut.DispatchInfo(AnalyzerSampleSampleData.CreatedAt);
        return sut;
    }
}
