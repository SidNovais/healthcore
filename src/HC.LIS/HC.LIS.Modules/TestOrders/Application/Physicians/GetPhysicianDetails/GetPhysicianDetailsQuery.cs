using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.GetPhysicianDetails;

public class GetPhysicianDetailsQuery(Guid physicianId) : QueryBase<PhysicianDetailsDto?>
{
    public Guid PhysicianId { get; } = physicianId;
}
