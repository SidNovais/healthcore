using System;
using System.Threading;
using System.Threading.Tasks;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemDetails;

namespace HC.LIS.Modules.LabAnalysis.Application.Contracts;

public interface IPdfGenerator
{
    Task<byte[]> GenerateAsync(
        WorklistItemDetailsDto worklistItemDetails,
        string signature,
        Guid signedBy,
        DateTime signedAt,
        CancellationToken cancellationToken);
}
