using System;
using System.Threading;
using System.Threading.Tasks;

namespace HC.LIS.Modules.TestOrders.Domain.Physicians;

/// <summary>
/// Provides the registry facts an order needs about its referring physician.
/// Defined in the Domain layer so that order creation can declare what it needs
/// without taking a dependency on infrastructure or application-layer DTOs.
/// Implemented in Infrastructure.
/// </summary>
public interface IRequestingPhysicianProvider
{
    Task<RequestingPhysician?> GetByIdAsync(Guid physicianId, CancellationToken cancellationToken);
}
