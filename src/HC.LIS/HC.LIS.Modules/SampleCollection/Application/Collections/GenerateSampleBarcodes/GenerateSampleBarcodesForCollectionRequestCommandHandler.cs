using HC.Core.Domain;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.SampleCollection.Application.Configuration.Commands;
using HC.LIS.Modules.SampleCollection.Domain.Collections;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.GenerateSampleBarcodes;

internal class GenerateSampleBarcodesForCollectionRequestCommandHandler(
    IAggregateStore aggregateStore,
    IBarcodeValueGenerator barcodeValueGenerator
) : ICommandHandler<GenerateSampleBarcodesForCollectionRequestCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;
    private readonly IBarcodeValueGenerator _barcodeValueGenerator = barcodeValueGenerator;

    public async Task Handle(
        GenerateSampleBarcodesForCollectionRequestCommand command,
        CancellationToken cancellationToken
    )
    {
        CollectionRequest? collectionRequest = await _aggregateStore
            .Load<CollectionRequest>(new CollectionRequestId(command.CollectionRequestId))
            .ConfigureAwait(false);

        foreach (string tubeType in collectionRequest!.GetPendingSampleTubeTypes())
        {
            collectionRequest.CreateBarcode(tubeType, _barcodeValueGenerator.Generate(), SystemClock.Now);
        }

        _aggregateStore.AppendChanges(collectionRequest);
    }
}
