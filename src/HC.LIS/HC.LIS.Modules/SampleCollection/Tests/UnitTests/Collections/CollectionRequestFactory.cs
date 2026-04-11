using System;
using System.Linq;
using HC.LIS.Modules.SampleCollection.Domain.Collections;
using HC.LIS.Modules.SampleCollection.Domain.Collections.Events;

namespace HC.LIS.Modules.SampleCollection.UnitTests.Collections;

internal static class CollectionRequestFactory
{
    public static CollectionRequest Create() =>
        CollectionRequest.Create(
            CollectionRequestSampleData.CollectionRequestId,
            CollectionRequestSampleData.PatientId,
            CollectionRequestSampleData.ExamPreparationVerified,
            isUrgent: false,
            CollectionRequestSampleData.ArrivedAt
        );

    /// <summary>
    /// Adds two exams of the same tube type and returns the auto-generated SampleId.
    /// </summary>
    public static Guid AddExams(CollectionRequest request)
    {
        request.AddExam(CollectionRequestSampleData.ExamId1, CollectionRequestSampleData.TubeType, CollectionRequestSampleData.ExamMnemonic1);
        request.AddExam(CollectionRequestSampleData.ExamId2, CollectionRequestSampleData.TubeType, CollectionRequestSampleData.ExamMnemonic2);
        return request.GetDomainEvents()
            .OfType<SampleCreatedForExamDomainEvent>()
            .Single()
            .SampleId;
    }
}
