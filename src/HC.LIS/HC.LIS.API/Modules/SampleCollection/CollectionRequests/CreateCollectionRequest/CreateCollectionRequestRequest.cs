namespace HC.LIS.API.Modules.SampleCollection.CollectionRequests.CreateCollectionRequest;

internal sealed record CreateCollectionRequestRequest(
    Guid CollectionRequestId,
    Guid PatientId,
    bool ExamPreparationVerified);
