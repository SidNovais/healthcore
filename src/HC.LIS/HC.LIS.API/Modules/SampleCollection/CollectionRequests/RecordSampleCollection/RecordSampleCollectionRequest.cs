namespace HC.LIS.API.Modules.SampleCollection.CollectionRequests.RecordSampleCollection;

internal sealed record RecordSampleCollectionRequest(
    Guid SampleId,
    Guid TechnicianId,
    string PatientName,
    DateTime PatientBirthdate,
    string PatientGender);
