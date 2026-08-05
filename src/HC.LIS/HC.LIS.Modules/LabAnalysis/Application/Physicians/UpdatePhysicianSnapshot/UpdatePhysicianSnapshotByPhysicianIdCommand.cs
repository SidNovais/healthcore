using Newtonsoft.Json;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Physicians.UpdatePhysicianSnapshot;

[method: JsonConstructor]
public class UpdatePhysicianSnapshotByPhysicianIdCommand(
    Guid id,
    Guid physicianId,
    string fullName,
    string? licenceNumber,
    DateTime updatedAt
) : InternalCommandBase(id)
{
    public Guid PhysicianId { get; } = physicianId;
    public string FullName { get; } = fullName;
    public string? LicenceNumber { get; } = licenceNumber;
    public DateTime UpdatedAt { get; } = updatedAt;
}
