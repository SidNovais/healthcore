using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Events;

public class ReportGeneratedDomainEvent(
    Guid worklistItemId,
    string reportPath,
    DateTime generatedAt
) : DomainEvent
{
    public Guid WorklistItemId { get; } = worklistItemId;
    public string ReportPath { get; } = reportPath;
    public DateTime GeneratedAt { get; } = generatedAt;
}
