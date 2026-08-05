using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.DeactivatePhysician;

public class DeactivatePhysicianCommand(
    Guid physicianId,
    DateTime deactivatedAt
) : CommandBase
{
    public Guid PhysicianId { get; } = physicianId;
    public DateTime DeactivatedAt { get; } = deactivatedAt;
}
