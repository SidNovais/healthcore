using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.PatientManagement.Domain.Patients.Events;

public class PatientAnonymizedDomainEvent(
    Guid patientId,
    DateTime anonymizedAt
) : DomainEvent
{
    public Guid PatientId { get; } = patientId;
    public DateTime AnonymizedAt { get; } = anonymizedAt;
}
