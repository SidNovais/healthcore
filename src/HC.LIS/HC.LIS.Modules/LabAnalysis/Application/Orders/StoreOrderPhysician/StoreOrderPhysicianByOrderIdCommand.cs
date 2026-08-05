using Newtonsoft.Json;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Orders.StoreOrderPhysician;

[method: JsonConstructor]
public class StoreOrderPhysicianByOrderIdCommand(
    Guid id,
    Guid orderId,
    Guid physicianId,
    DateTime requestedAt
) : InternalCommandBase(id)
{
    public Guid OrderId { get; } = orderId;
    public Guid PhysicianId { get; } = physicianId;
    public DateTime RequestedAt { get; } = requestedAt;
}
