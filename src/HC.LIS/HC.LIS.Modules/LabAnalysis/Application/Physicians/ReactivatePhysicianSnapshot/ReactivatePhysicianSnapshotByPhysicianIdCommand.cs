using Newtonsoft.Json;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Physicians.ReactivatePhysicianSnapshot;

[method: JsonConstructor]
public class ReactivatePhysicianSnapshotByPhysicianIdCommand(
    Guid id,
    Guid physicianId,
    DateTime reactivatedAt
) : InternalCommandBase(id)
{
    public Guid PhysicianId { get; } = physicianId;
    public DateTime ReactivatedAt { get; } = reactivatedAt;
}
