using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.SampleCollection.Application.Configuration.Commands;
using HC.LIS.Modules.SampleCollection.Domain.Collections;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.CreateCollectionRequest;

internal class CreateCollectionRequestCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<CreateCollectionRequestCommand, Guid>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public Task<Guid> Handle(
        CreateCollectionRequestCommand command,
        CancellationToken cancellationToken
    )
    {
        CollectionRequest request = CollectionRequest.Create(
            command.CollectionRequestId,
            command.PatientId,
            command.ExamPreparationVerified,
            command.ArrivedAt
        );
        _aggregateStore.Start(request);
        return Task.FromResult(command.CollectionRequestId);
    }
}
