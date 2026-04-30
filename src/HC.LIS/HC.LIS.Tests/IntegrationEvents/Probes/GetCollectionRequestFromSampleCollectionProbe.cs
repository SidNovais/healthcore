using System;
using System.Threading.Tasks;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.SampleCollection.Application.Collections.GetCollectionRequestByPatientId;
using HC.LIS.Modules.SampleCollection.Application.Collections.GetCollectionRequestDetails;
using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.Tests.IntegrationEvents.Probes;

public sealed class GetCollectionRequestFromSampleCollectionProbe(
    Guid patientId,
    ISampleCollectionModule module
) : IProbe<CollectionRequestDetailsDto>
{
    public string DescribeFailureTo() =>
        $"CollectionRequest for PatientId {patientId} not found in SampleCollection";

    public async Task<CollectionRequestDetailsDto?> GetSampleAsync() =>
        await module
            .ExecuteQueryAsync(new GetCollectionRequestByPatientIdQuery(patientId))
            .ConfigureAwait(false);

    public bool IsSatisfied(CollectionRequestDetailsDto? sample) => sample is not null;
}
