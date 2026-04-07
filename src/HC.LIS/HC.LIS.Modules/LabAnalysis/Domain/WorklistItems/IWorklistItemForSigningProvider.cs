using System;
using System.Threading;
using System.Threading.Tasks;

namespace HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;

/// <summary>
/// Provides the domain data required to initiate the signing of a worklist item's report.
/// Defined in the Domain layer so that domain logic can declare what it needs without
/// taking a dependency on infrastructure or application-layer DTOs.
/// Implemented in Infrastructure.
/// </summary>
public interface IWorklistItemForSigningProvider
{
    Task<WorklistItemForSigning?> GetAsync(Guid worklistItemId, CancellationToken cancellationToken);
}
