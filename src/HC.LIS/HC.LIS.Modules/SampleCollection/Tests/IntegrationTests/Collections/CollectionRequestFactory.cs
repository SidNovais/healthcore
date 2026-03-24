using System.Threading.Tasks;
using HC.LIS.Modules.SampleCollection.Application.Collections.CreateCollectionRequest;
using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.Modules.SampleCollection.IntegrationTests.Collections;

internal static class CollectionRequestFactory
{
    public static async Task CreateAsync(ISampleCollectionModule sampleCollectionModule)
    {
        await sampleCollectionModule.ExecuteCommandAsync(
            new CreateCollectionRequestCommand(
                CollectionRequestSampleData.CollectionRequestId,
                CollectionRequestSampleData.PatientId,
                true,
                CollectionRequestSampleData.ArrivedAt
            )
        ).ConfigureAwait(false);
    }
}
