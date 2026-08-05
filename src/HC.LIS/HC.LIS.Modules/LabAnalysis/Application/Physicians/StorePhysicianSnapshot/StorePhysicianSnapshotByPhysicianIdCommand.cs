using Newtonsoft.Json;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Physicians.StorePhysicianSnapshot;

[method: JsonConstructor]
public class StorePhysicianSnapshotByPhysicianIdCommand(
    Guid id,
    Guid physicianId,
    string fullName,
    string? licenceNumber,
    DateTime registeredAt
) : InternalCommandBase(id)
{
    public Guid PhysicianId { get; } = physicianId;
    public string FullName { get; } = fullName;
    public string? LicenceNumber { get; } = licenceNumber;
    public DateTime RegisteredAt { get; } = registeredAt;
}
