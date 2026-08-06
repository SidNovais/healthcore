using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.SampleCollection.Application.Collections.GetSamplesByCollectionRequestId;
using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.Tests.IntegrationEvents.Probes;

public sealed class GetGeneratedBarcodeFromSampleCollectionProbe(
    Guid collectionRequestId,
    Guid sampleId,
    ISampleCollectionModule module
) : IProbe<SampleSummaryDto>
{
    public string DescribeFailureTo() =>
        $"Sample {sampleId} of CollectionRequest {collectionRequestId} has no generated barcode";

    public async Task<SampleSummaryDto?> GetSampleAsync()
    {
        IReadOnlyCollection<SampleSummaryDto>? samples = await module
            .ExecuteQueryAsync(new GetSamplesByCollectionRequestIdQuery(collectionRequestId))
            .ConfigureAwait(false);

        return samples?.SingleOrDefault(sample => sample.Id == sampleId);
    }

    public bool IsSatisfied(SampleSummaryDto? sample) =>
        !string.IsNullOrEmpty(sample?.Barcode);
}
