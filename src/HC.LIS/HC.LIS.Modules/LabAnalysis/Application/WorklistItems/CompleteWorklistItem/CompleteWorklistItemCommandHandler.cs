using System.Threading;
using System.Threading.Tasks;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.CompleteWorklistItem;

internal class CompleteWorklistItemCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<CompleteWorklistItemCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task Handle(
        CompleteWorklistItemCommand command,
        CancellationToken cancellationToken
    )
    {
        WorklistItem? worklistItem = await _aggregateStore
            .Load<WorklistItem>(new WorklistItemId(command.WorklistItemId))
            .ConfigureAwait(false);
        worklistItem!.Complete(command.CompletionType, command.CompletedAt);
        _aggregateStore.AppendChanges(worklistItem);
    }
}
