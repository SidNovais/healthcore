using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.AddExamToCollection;

public class AddExamToCollectionCommand(
    Guid collectionRequestId,
    Guid examId,
    string examMnemonic,
    string tubeType
) : CommandBase
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
    public Guid ExamId { get; } = examId;
    public string ExamMnemonic { get; } = examMnemonic;
    public string TubeType { get; } = tubeType;
}
