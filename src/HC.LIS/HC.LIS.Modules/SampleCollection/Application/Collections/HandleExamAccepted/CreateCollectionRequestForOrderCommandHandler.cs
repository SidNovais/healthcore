using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.SampleCollection.Application.Configuration.Commands;
using HC.LIS.Modules.SampleCollection.Domain.Collections;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.HandleExamAccepted;

internal class CreateCollectionRequestForOrderCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<CreateCollectionRequestForOrderCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public Task Handle(
        CreateCollectionRequestForOrderCommand command,
        CancellationToken cancellationToken
    )
    {
        CollectionRequest collectionRequest = CollectionRequest.Create(
            command.CollectionRequestId,
            command.PatientId,
            false,
            command.AcceptedAt
        );
        collectionRequest.AddExam(command.ExamId, command.ContainerType, command.ExamMnemonic);
        _aggregateStore.Start(collectionRequest);
        return Task.CompletedTask;
    }
}
