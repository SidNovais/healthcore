using System;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;

namespace HC.LIS.Modules.LabAnalysis.Application.SignedReports.GetSignedReportDetails;


public class GetSignedReportByWorklistItemIdQuery(Guid worklistItemId) : QueryBase<SignedReportDetailsDto?>
{
    public Guid WorklistItemId { get; } = worklistItemId;
}
