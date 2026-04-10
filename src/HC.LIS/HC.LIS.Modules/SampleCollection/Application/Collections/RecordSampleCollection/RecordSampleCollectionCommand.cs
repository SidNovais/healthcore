using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.RecordSampleCollection;

public class RecordSampleCollectionCommand(
    Guid collectionRequestId,
    Guid sampleId,
    Guid technicianId,
    string patientName,
    DateTime patientBirthdate,
    string patientGender,
    DateTime collectedAt
) : CommandBase
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
    public Guid SampleId { get; } = sampleId;
    public Guid TechnicianId { get; } = technicianId;
    public string PatientName { get; } = patientName;
    public DateTime PatientBirthdate { get; } = patientBirthdate;
    public string PatientGender { get; } = patientGender;
    public DateTime CollectedAt { get; } = collectedAt;
}
