using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.CreateCollectionRequest;

public class CreateCollectionRequestCommand(
    Guid collectionRequestId,
    Guid patientId,
    Guid orderId,
    bool examPreparationVerified,
    DateTime arrivedAt
) : CommandBase<Guid>
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
    public Guid PatientId { get; } = patientId;
    public Guid OrderId { get; } = orderId;
    public bool ExamPreparationVerified { get; } = examPreparationVerified;
    public DateTime ArrivedAt { get; } = arrivedAt;
}
