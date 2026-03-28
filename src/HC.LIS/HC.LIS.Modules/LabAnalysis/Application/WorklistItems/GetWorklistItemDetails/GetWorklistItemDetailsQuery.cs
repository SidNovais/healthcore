using System;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemDetails;

public class GetWorklistItemDetailsQuery(Guid worklistItemId) : QueryBase<WorklistItemDetailsDto?>
{
    public Guid WorklistItemId { get; } = worklistItemId;
}
