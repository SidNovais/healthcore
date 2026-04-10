using System;
using HC.Core.Infrastructure.EventBus;

namespace HC.LIS.Modules.SampleCollection.IntegrationEvents;

public class SampleCollectedIntegrationEvent(
    Guid id,
    DateTime occurredAt,
    Guid collectionRequestId,
    Guid sampleId,
    Guid patientId,
    string sampleBarcode,
    string patientName,
    DateTime patientBirthdate,
    string patientGender,
    IReadOnlyCollection<ExamInfo> exams
) : IntegrationEvent(id, occurredAt)
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
    public Guid SampleId { get; } = sampleId;
    public Guid PatientId { get; } = patientId;
    public string SampleBarcode { get; } = sampleBarcode;
    public string PatientName { get; } = patientName;
    public DateTime PatientBirthdate { get; } = patientBirthdate;
    public string PatientGender { get; } = patientGender;
    public IReadOnlyCollection<ExamInfo> Exams { get; } = exams;
}
