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

    public Task<Guid> Handle(
        CreateWorklistItemCommand command,
        CancellationToken cancellationToken
    )
    {
        WorklistItem worklistItem = WorklistItem.Create(
            command.WorklistItemId,
            command.SampleId,
            command.SampleBarcode,
            command.ExamCode,
            command.PatientId,
            command.CreatedAt
        );
        _aggregateStore.Start(worklistItem);
        return Task.FromResult(command.WorklistItemId);
    }
}
