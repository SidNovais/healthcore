using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections;

public class CollectionExam : ValueObject
{
    public Guid ExamId { get; }
    public string TubeType { get; }

    private CollectionExam(Guid examId, string tubeType)
    {
        ExamId = examId;
        TubeType = tubeType;
    }

    public static CollectionExam Of(Guid examId, string tubeType) => new(examId, tubeType);
}
