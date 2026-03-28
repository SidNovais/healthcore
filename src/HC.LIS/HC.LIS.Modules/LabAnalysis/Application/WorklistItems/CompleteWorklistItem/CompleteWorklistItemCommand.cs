using System;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.CompleteWorklistItem;

public class CompleteWorklistItemCommand(
    Guid worklistItemId,
    string completionType,
    DateTime completedAt
) : CommandBase
{
    public Guid WorklistItemId { get; } = worklistItemId;
    public string CompletionType { get; } = completionType;
    public DateTime CompletedAt { get; } = completedAt;
}
