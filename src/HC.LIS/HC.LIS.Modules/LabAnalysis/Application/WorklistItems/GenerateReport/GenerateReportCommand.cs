using System;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GenerateReport;

public class GenerateReportCommand(
    Guid worklistItemId,
    string reportPath,
    DateTime generatedAt
) : CommandBase
{
    public Guid WorklistItemId { get; } = worklistItemId;
    public string ReportPath { get; } = reportPath;
    public DateTime GeneratedAt { get; } = generatedAt;
}
