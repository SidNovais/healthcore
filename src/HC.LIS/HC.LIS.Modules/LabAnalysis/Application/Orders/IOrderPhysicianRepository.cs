using HC.LIS.Modules.LabAnalysis.Application.Physicians;

namespace HC.LIS.Modules.LabAnalysis.Application.Orders;

public interface IOrderPhysicianRepository
{
    /// <summary>
    /// Returns the physician who requested an order, or <c>null</c> when no mapping is stored yet —
    /// orders placed before the registry existed never get one.
    /// </summary>
    Task<PhysicianSnapshotView?> GetRequestingPhysicianAsync(Guid orderId);

    Task StoreAsync(Guid orderId, Guid physicianId, DateTime requestedAt);
}
