using System;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.CompleteWorklistItem;

public class CompleteWorklistItemCommand(
    Guid worklistItemId,
    DateTime completedAt
) : CommandBase
{
    public Guid WorklistItemId { get; } = worklistItemId;
    public DateTime CompletedAt { get; } = completedAt;
}
