using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.SampleCollection.Application.Configuration.Commands;
using HC.LIS.Modules.SampleCollection.Domain.Collections;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.RecordSampleCollection;

internal class RecordSampleCollectionCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<RecordSampleCollectionCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task Handle(
        RecordSampleCollectionCommand command,
        CancellationToken cancellationToken
    )
    {
        CollectionRequest? request = await _aggregateStore
            .Load<CollectionRequest>(new CollectionRequestId(command.CollectionRequestId))
            .ConfigureAwait(false);
        request!.RecordCollection(command.SampleId, command.TechnicianId, command.CollectedAt);
        _aggregateStore.AppendChanges(request);
    }
}
