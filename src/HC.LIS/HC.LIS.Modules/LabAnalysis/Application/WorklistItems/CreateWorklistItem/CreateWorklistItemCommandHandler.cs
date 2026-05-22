using System;
using System.Threading;
using System.Threading.Tasks;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.CreateWorklistItem;

internal class CreateWorklistItemCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<CreateWorklistItemCommand, Guid>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task<Guid> Handle(
        CreateWorklistItemCommand command,
        CancellationToken cancellationToken
    )
    {
        WorklistItem? existing = await _aggregateStore
            .Load<WorklistItem>(new WorklistItemId(command.WorklistItemId))
            .ConfigureAwait(false);
        if (existing is not null)
            return command.WorklistItemId;

        WorklistItem worklistItem = WorklistItem.Create(
            command.WorklistItemId,
            command.SampleId,
            command.SampleBarcode,
            command.ExamCode,
            command.PatientId,
            command.OrderId,
            command.OrderItemId,
            command.CreatedAt
        );
        _aggregateStore.Start(worklistItem);
        return command.WorklistItemId;
    }
}
