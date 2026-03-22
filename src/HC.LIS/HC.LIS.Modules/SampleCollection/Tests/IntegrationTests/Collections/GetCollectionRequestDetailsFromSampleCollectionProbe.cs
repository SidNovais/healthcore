using System;
using System.Threading.Tasks;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.SampleCollection.Application.Collections.GetCollectionRequestDetails;
using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.Modules.SampleCollection.IntegrationTests.Collections;

public class GetCollectionRequestDetailsFromSampleCollectionProbe(
    Guid expectedCollectionRequestId,
    ISampleCollectionModule sampleCollectionModule
) : IProbe<CollectionRequestDetailsDto>
{
    private readonly Guid _expectedCollectionRequestId = expectedCollectionRequestId;
    private readonly ISampleCollectionModule _sampleCollectionModule = sampleCollectionModule;

    public string DescribeFailureTo() =>
        $"CollectionRequestDetails not found for {_expectedCollectionRequestId}";

    public async Task<CollectionRequestDetailsDto?> GetSampleAsync()
    {
        return await _sampleCollectionModule
            .ExecuteQueryAsync(new GetCollectionRequestDetailsQuery(_expectedCollectionRequestId))
            .ConfigureAwait(false);
    }

    public bool IsSatisfied(CollectionRequestDetailsDto? sample) => sample is not null;
}
