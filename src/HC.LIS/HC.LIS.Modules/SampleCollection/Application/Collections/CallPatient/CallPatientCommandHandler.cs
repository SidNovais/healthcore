using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.SampleCollection.Application.Configuration.Commands;
using HC.LIS.Modules.SampleCollection.Domain.Collections;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.CallPatient;

internal class CallPatientCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<CallPatientCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task Handle(
        CallPatientCommand command,
        CancellationToken cancellationToken
    )
    {
        CollectionRequest? request = await _aggregateStore
            .Load<CollectionRequest>(new CollectionRequestId(command.CollectionRequestId))
            .ConfigureAwait(false);
        request!.CallPatient(command.TechnicianId, command.CalledAt);
        _aggregateStore.AppendChanges(request);
    }
}
