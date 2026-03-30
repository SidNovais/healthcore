using System;
using HC.Core.Infrastructure.EventBus;

namespace HC.LIS.Modules.LabAnalysis.IntegrationEvents;

public class AnalyzerResultReceivedIntegrationEvent(
    Guid id,
    DateTime occurredAt,
    Guid worklistItemId,
    Guid instrumentId,
    string resultValue,
    string resultUnit,
    string referenceRange,
    DateTime recordedAt
) : IntegrationEvent(id, occurredAt)
{
    public Guid WorklistItemId { get; } = worklistItemId;
    public Guid InstrumentId { get; } = instrumentId;
    public string ResultValue { get; } = resultValue;
    public string ResultUnit { get; } = resultUnit;
    public string ReferenceRange { get; } = referenceRange;
    public DateTime RecordedAt { get; } = recordedAt;
}
