using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections.Events;

public class SampleCreatedForExamDomainEvent(
    Guid collectionRequestId,
    Guid sampleId,
    Guid examId,
    string tubeType,
    string examMnemonic
) : DomainEvent
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
    public Guid SampleId { get; } = sampleId;
    public Guid ExamId { get; } = examId;
    public string TubeType { get; } = tubeType;
    public string ExamMnemonic { get; } = examMnemonic;
}
