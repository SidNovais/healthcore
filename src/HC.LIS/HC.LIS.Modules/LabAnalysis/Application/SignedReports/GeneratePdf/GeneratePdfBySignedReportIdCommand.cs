using System;
using Newtonsoft.Json;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.SignedReports.GeneratePdf;

[method: JsonConstructor]
public class GeneratePdfBySignedReportIdCommand(
    Guid id,
    Guid reportId,
    Guid worklistItemId
) : InternalCommandBase(id)
{
    public Guid ReportId { get; } = reportId;
    public Guid WorklistItemId { get; } = worklistItemId;
}
