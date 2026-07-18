using System;
using HC.Core.Infrastructure.EventBus;

namespace HC.LIS.Modules.LabAnalysis.IntegrationEvents;

public class WorklistItemResultRecordedIntegrationEvent(
    Guid id,
    DateTime occurredAt,
    Guid worklistItemId
) : IntegrationEvent(id, occurredAt)
{
    public Guid WorklistItemId { get; } = worklistItemId;
}
