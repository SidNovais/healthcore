using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections.Events;

public class ExamAddedToExistingSampleDomainEvent(
    Guid collectionRequestId,
    Guid sampleId,
    Guid examId
) : DomainEvent
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
    public Guid SampleId { get; } = sampleId;
    public Guid ExamId { get; } = examId;
}
