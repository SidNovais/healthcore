using System.Collections.Generic;
using System.Threading.Tasks;
using HC.Core.Domain;
using HC.LIS.Modules.Analyzer.Application.Contracts;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.CreateAnalyzerSample;

namespace HC.LIS.Modules.Analyzer.IntegrationTests.AnalyzerSamples;

internal static class AnalyzerSampleFactory
{
    public static async Task CreateAsync(
        IAnalyzerModule analyzerModule,
        IReadOnlyCollection<ExamInfoDto>? exams = null
    )
    {
        await analyzerModule.ExecuteCommandAsync(new CreateAnalyzerSampleCommand(
            AnalyzerSampleSampleData.AnalyzerSampleId,
            AnalyzerSampleSampleData.SampleId,
            AnalyzerSampleSampleData.PatientId,
            AnalyzerSampleSampleData.SampleBarcode,
            AnalyzerSampleSampleData.PatientName,
            AnalyzerSampleSampleData.PatientBirthdate,
            AnalyzerSampleSampleData.PatientGender,
            exams ?? new List<ExamInfoDto>
            {
                new(AnalyzerSampleSampleData.ExamId1, AnalyzerSampleSampleData.ExamMnemonic1)
            }.AsReadOnly(),
            SystemClock.Now
        )).ConfigureAwait(false);
    }
}
