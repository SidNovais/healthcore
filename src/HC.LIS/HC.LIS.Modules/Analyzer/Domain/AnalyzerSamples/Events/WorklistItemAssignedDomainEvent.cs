using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples.Events;

public class WorklistItemAssignedDomainEvent(
    Guid analyzerSampleId,
    string examMnemonic,
    Guid worklistItemId,
    DateTime assignedAt
) : DomainEvent
{
    public Guid AnalyzerSampleId { get; } = analyzerSampleId;
    public string ExamMnemonic { get; } = examMnemonic;
    public Guid WorklistItemId { get; } = worklistItemId;
    public DateTime AssignedAt { get; } = assignedAt;
}
