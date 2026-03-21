using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.SampleCollection.Application.Configuration.Commands;
using HC.LIS.Modules.SampleCollection.Domain.Collections;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.MovePatientToWaiting;

internal class MovePatientToWaitingCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<MovePatientToWaitingCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task Handle(
        MovePatientToWaitingCommand command,
        CancellationToken cancellationToken
    )
    {
        CollectionRequest? request = await _aggregateStore
            .Load<CollectionRequest>(new CollectionRequestId(command.CollectionRequestId))
            .ConfigureAwait(false);
        request!.MoveToWaiting(command.WaitingAt);
        _aggregateStore.AppendChanges(request);
    }
}
