using System;
using HC.Core.Infrastructure.EventBus;

namespace HC.LIS.Modules.Analyzer.IntegrationEvents;

public class ExamResultReceivedIntegrationEvent(
    Guid id,
    DateTime occurredAt,
    Guid analyzerSampleId,
    Guid worklistItemId,
    string examMnemonic,
    Guid instrumentId,
    string resultValue,
    string resultUnit,
    string referenceRange,
    DateTime recordedAt
) : IntegrationEvent(id, occurredAt)
{
    public Guid AnalyzerSampleId { get; } = analyzerSampleId;
    public Guid WorklistItemId { get; } = worklistItemId;
    public string ExamMnemonic { get; } = examMnemonic;
    public Guid InstrumentId { get; } = instrumentId;
    public string ResultValue { get; } = resultValue;
    public string ResultUnit { get; } = resultUnit;
    public string ReferenceRange { get; } = referenceRange;
    public DateTime RecordedAt { get; } = recordedAt;
}
