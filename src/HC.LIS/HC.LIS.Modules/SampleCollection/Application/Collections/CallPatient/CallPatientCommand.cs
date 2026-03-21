using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.CallPatient;

public class CallPatientCommand(
    Guid collectionRequestId,
    Guid technicianId,
    DateTime calledAt
) : CommandBase
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
    public Guid TechnicianId { get; } = technicianId;
    public DateTime CalledAt { get; } = calledAt;
}
