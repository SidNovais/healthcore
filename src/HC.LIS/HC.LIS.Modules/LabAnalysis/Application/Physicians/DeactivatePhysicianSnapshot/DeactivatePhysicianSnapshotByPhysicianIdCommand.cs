using Newtonsoft.Json;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Physicians.DeactivatePhysicianSnapshot;

[method: JsonConstructor]
public class DeactivatePhysicianSnapshotByPhysicianIdCommand(
    Guid id,
    Guid physicianId,
    DateTime deactivatedAt
) : InternalCommandBase(id)
{
    public Guid PhysicianId { get; } = physicianId;
    public DateTime DeactivatedAt { get; } = deactivatedAt;
}
