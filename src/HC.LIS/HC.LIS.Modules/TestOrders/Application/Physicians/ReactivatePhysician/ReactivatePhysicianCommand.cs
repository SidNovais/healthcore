using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.ReactivatePhysician;

public class ReactivatePhysicianCommand(
    Guid physicianId,
    DateTime reactivatedAt
) : CommandBase
{
    public Guid PhysicianId { get; } = physicianId;
    public DateTime ReactivatedAt { get; } = reactivatedAt;
}
