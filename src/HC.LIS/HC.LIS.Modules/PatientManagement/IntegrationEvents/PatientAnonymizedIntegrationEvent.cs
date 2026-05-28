using HC.Core.Infrastructure.EventBus;

namespace HC.LIS.Modules.PatientManagement.IntegrationEvents;

public class PatientAnonymizedIntegrationEvent(
    Guid id,
    DateTime occurredAt,
    Guid patientId,
    DateTime anonymizedAt
) : IntegrationEvent(id, occurredAt)
{
    public Guid PatientId { get; } = patientId;
    public DateTime AnonymizedAt { get; } = anonymizedAt;
}
