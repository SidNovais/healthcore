using System;
using System.Text.Json.Serialization;
using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections;

public class CollectionExam : ValueObject
{
    public Guid ExamId { get; }
    public string TubeType { get; }
    public string ExamMnemonic { get; }

    [JsonConstructor]
    private CollectionExam(Guid examId, string tubeType, string examMnemonic)
    {
        ExamId = examId;
        TubeType = tubeType;
        ExamMnemonic = examMnemonic;
    }

    public static CollectionExam Of(Guid examId, string tubeType, string examMnemonic) => new(examId, tubeType, examMnemonic);
}
