using Newtonsoft.Json;
using HC.LIS.Modules.SampleCollection.Application.Configuration.Commands;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.HandleExamAccepted;

[method: JsonConstructor]
public class CreateCollectionRequestForOrderCommand(
    Guid id,
    Guid collectionRequestId,
    Guid patientId,
    Guid examId,
    string containerType,
    DateTime acceptedAt
) : InternalCommandBase(id)
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
    public Guid PatientId { get; } = patientId;
    public Guid ExamId { get; } = examId;
    public string ContainerType { get; } = containerType;
    public DateTime AcceptedAt { get; } = acceptedAt;
}
