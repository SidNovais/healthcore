using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.RegisterPhysician;

public class RegisterPhysicianCommand(
    Guid physicianId,
    string fullName,
    string? licenceNumber,
    DateTime registeredAt
) : CommandBase<Guid>
{
    public Guid PhysicianId { get; } = physicianId;
    public string FullName { get; } = fullName;
    public string? LicenceNumber { get; } = licenceNumber;
    public DateTime RegisteredAt { get; } = registeredAt;
}
