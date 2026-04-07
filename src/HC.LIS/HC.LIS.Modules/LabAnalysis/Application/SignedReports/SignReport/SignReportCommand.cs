using System;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;

namespace HC.LIS.Modules.LabAnalysis.Application.SignedReports.SignReport;

public class SignReportCommand(
    Guid worklistItemId,
    string signature,
    Guid signedBy
) : CommandBase<Guid>
{
    public Guid WorklistItemId { get; } = worklistItemId;
    public string Signature { get; } = signature;
    public Guid SignedBy { get; } = signedBy;
}
