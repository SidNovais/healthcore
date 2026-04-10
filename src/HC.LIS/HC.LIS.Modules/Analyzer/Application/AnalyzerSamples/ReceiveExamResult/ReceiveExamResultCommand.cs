using HC.LIS.Modules.Analyzer.Application.Contracts;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.ReceiveExamResult;

public class ReceiveExamResultCommand(
    Guid analyzerSampleId,
    string examMnemonic,
    string resultValue,
    string resultUnit,
    string referenceRange,
    Guid instrumentId,
    DateTime recordedAt
) : CommandBase
{
    public Guid AnalyzerSampleId { get; } = analyzerSampleId;
    public string ExamMnemonic { get; } = examMnemonic;
    public string ResultValue { get; } = resultValue;
    public string ResultUnit { get; } = resultUnit;
    public string ReferenceRange { get; } = referenceRange;
    public Guid InstrumentId { get; } = instrumentId;
    public DateTime RecordedAt { get; } = recordedAt;
}
