using Newtonsoft.Json;
using HC.LIS.Modules.SampleCollection.Application.Configuration.Commands;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.HandleExamAccepted;

[method: JsonConstructor]
public class AddExamToCollectionForOrderCommand(
    Guid id,
    Guid collectionRequestId,
    Guid examId,
    string containerType
) : InternalCommandBase(id)
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
    public Guid ExamId { get; } = examId;
    public string ContainerType { get; } = containerType;
}
