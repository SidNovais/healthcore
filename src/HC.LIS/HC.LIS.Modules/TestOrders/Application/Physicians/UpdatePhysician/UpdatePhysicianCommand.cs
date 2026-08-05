using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.UpdatePhysician;

public class UpdatePhysicianCommand(
    Guid physicianId,
    string fullName,
    string? licenceNumber,
    DateTime updatedAt
) : CommandBase
{
    public Guid PhysicianId { get; } = physicianId;
    public string FullName { get; } = fullName;
    public string? LicenceNumber { get; } = licenceNumber;
    public DateTime UpdatedAt { get; } = updatedAt;
}
