using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections.Events;

public class PatientArrivedDomainEvent(
    Guid collectionRequestId,
    Guid patientId,
    bool examPreparationVerified,
    DateTime arrivedAt
) : DomainEvent
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
    public Guid PatientId { get; } = patientId;
    public bool ExamPreparationVerified { get; } = examPreparationVerified;
    public DateTime ArrivedAt { get; } = arrivedAt;
}
