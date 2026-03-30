using System;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GenerateReport;

public class GenerateReportCommand(
    Guid id,
    Guid worklistItemId,
    string reportPath,
    DateTime generatedAt
) : InternalCommandBase(id)
{
    public Guid WorklistItemId { get; } = worklistItemId;
    public string ReportPath { get; } = reportPath;
    public DateTime GeneratedAt { get; } = generatedAt;
}
