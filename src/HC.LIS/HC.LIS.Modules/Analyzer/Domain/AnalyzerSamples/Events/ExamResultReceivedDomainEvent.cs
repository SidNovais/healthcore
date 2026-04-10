using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples.Events;

public class ExamResultReceivedDomainEvent(
    Guid analyzerSampleId,
    string examMnemonic,
    Guid worklistItemId,
    string resultValue,
    string resultUnit,
    string referenceRange,
    Guid instrumentId,
    bool allResultsReceived,
    DateTime recordedAt
) : DomainEvent
{
    public Guid AnalyzerSampleId { get; } = analyzerSampleId;
    public string ExamMnemonic { get; } = examMnemonic;
    public Guid WorklistItemId { get; } = worklistItemId;
    public string ResultValue { get; } = resultValue;
    public string ResultUnit { get; } = resultUnit;
    public string ReferenceRange { get; } = referenceRange;
    public Guid InstrumentId { get; } = instrumentId;
    public bool AllResultsReceived { get; } = allResultsReceived;
    public DateTime RecordedAt { get; } = recordedAt;
}
