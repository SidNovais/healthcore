namespace HC.LIS.Modules.SampleCollection.Application.Collections.GetCollectionRequestList;

public record CollectionRequestSummaryDto(
    Guid CollectionRequestId,
    Guid PatientId,
    string Status,
    DateTime ArrivedAt,
    DateTime? WaitingAt,
    DateTime? CalledAt);
