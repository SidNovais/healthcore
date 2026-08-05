using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Physicians;

public class RequestingPhysician : ValueObject
{
    public PhysicianId PhysicianId { get; }
    public string FullName { get; }
    public bool IsActive { get; }

    private RequestingPhysician(PhysicianId physicianId, string fullName, bool isActive)
    {
        PhysicianId = physicianId;
        FullName = fullName;
        IsActive = isActive;
    }

    public static RequestingPhysician Of(PhysicianId physicianId, string fullName, bool isActive)
        => new(physicianId, fullName, isActive);
}
