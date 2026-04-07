using System;
using Newtonsoft.Json;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.SignedReports.CompleteWorklistItem;

[method: JsonConstructor]
public class CompleteWorklistItemBySignedReportCommand(
    Guid id,
    Guid worklistItemId,
    DateTime completedAt
) : InternalCommandBase(id)
{
    public Guid WorklistItemId { get; } = worklistItemId;
    public DateTime CompletedAt { get; } = completedAt;
}
