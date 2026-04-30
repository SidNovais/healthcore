using HC.LIS.Modules.SampleCollection.Application.Collections.GetCollectionRequestDetails;
using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.GetCollectionRequestByPatientId;

public class GetCollectionRequestByPatientIdQuery(Guid patientId)
    : QueryBase<CollectionRequestDetailsDto?>
{
    public Guid PatientId { get; } = patientId;
}
