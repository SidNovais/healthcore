using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.MovePatientToWaiting;

public class MovePatientToWaitingCommand(
    Guid collectionRequestId,
    DateTime waitingAt
) : CommandBase
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
    public DateTime WaitingAt { get; } = waitingAt;
}
