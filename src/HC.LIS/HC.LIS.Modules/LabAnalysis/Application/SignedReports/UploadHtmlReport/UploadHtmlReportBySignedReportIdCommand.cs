using System;
using Newtonsoft.Json;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.SignedReports.UploadHtmlReport;

[method: JsonConstructor]
public class UploadHtmlReportBySignedReportIdCommand(
    Guid id,
    Guid reportId,
    Guid worklistItemId,
    string signature,
    Guid signedBy,
    DateTime signedAt
) : InternalCommandBase(id)
{
    public Guid ReportId { get; } = reportId;
    public Guid WorklistItemId { get; } = worklistItemId;
    public string Signature { get; } = signature;
    public Guid SignedBy { get; } = signedBy;
    public DateTime SignedAt { get; } = signedAt;
}
