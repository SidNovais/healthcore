using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.SampleCollection.Application.Configuration.Commands;
using HC.LIS.Modules.SampleCollection.Domain.Collections;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.CreateBarcode;

internal class CreateBarcodeCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<CreateBarcodeCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task Handle(
        CreateBarcodeCommand command,
        CancellationToken cancellationToken
    )
    {
        CollectionRequest? request = await _aggregateStore
            .Load<CollectionRequest>(new CollectionRequestId(command.CollectionRequestId))
            .ConfigureAwait(false);
        request!.CreateBarcode(command.TubeType, command.BarcodeValue, command.TechnicianId, command.CreatedAt);
        _aggregateStore.AppendChanges(request);
    }
}
