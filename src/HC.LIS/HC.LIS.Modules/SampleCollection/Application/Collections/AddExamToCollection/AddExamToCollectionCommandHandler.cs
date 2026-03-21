using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.SampleCollection.Application.Configuration.Commands;
using HC.LIS.Modules.SampleCollection.Domain.Collections;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.AddExamToCollection;

internal class AddExamToCollectionCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<AddExamToCollectionCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task Handle(
        AddExamToCollectionCommand command,
        CancellationToken cancellationToken
    )
    {
        CollectionRequest? request = await _aggregateStore
            .Load<CollectionRequest>(new CollectionRequestId(command.CollectionRequestId))
            .ConfigureAwait(false);
        request!.AddExam(command.ExamId, command.TubeType);
        _aggregateStore.AppendChanges(request);
    }
}
