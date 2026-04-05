using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.SampleCollection.Application.Configuration.Commands;
using HC.LIS.Modules.SampleCollection.Domain.Collections;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.HandleExamAccepted;

internal class AddExamToCollectionForOrderCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<AddExamToCollectionForOrderCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task Handle(
        AddExamToCollectionForOrderCommand command,
        CancellationToken cancellationToken
    )
    {
        CollectionRequest? collectionRequest = await _aggregateStore
            .Load<CollectionRequest>(new CollectionRequestId(command.CollectionRequestId))
            .ConfigureAwait(false);
        collectionRequest!.AddExam(command.ExamId, command.ContainerType, command.ExamMnemonic);
        _aggregateStore.AppendChanges(collectionRequest);
    }
}
