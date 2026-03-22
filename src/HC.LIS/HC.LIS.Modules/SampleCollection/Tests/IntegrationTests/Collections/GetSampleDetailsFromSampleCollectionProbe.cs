using System;
using System.Threading.Tasks;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.SampleCollection.Application.Collections.GetSampleDetails;
using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.Modules.SampleCollection.IntegrationTests.Collections;

public class GetSampleDetailsFromSampleCollectionProbe(
    Guid expectedSampleId,
    ISampleCollectionModule sampleCollectionModule
) : IProbe<SampleDetailsDto>
{
    private readonly Guid _expectedSampleId = expectedSampleId;
    private readonly ISampleCollectionModule _sampleCollectionModule = sampleCollectionModule;

    public string DescribeFailureTo() =>
        $"SampleDetails not found for {_expectedSampleId}";

    public async Task<SampleDetailsDto?> GetSampleAsync()
    {
        return await _sampleCollectionModule
            .ExecuteQueryAsync(new GetSampleDetailsQuery(_expectedSampleId))
            .ConfigureAwait(false);
    }

    public bool IsSatisfied(SampleDetailsDto? sample) => sample is not null;
}
