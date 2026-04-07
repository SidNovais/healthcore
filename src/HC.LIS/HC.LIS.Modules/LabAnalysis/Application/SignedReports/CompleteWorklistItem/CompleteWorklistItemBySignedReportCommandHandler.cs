using System.Threading;
using System.Threading.Tasks;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;

namespace HC.LIS.Modules.LabAnalysis.Application.SignedReports.CompleteWorklistItem;

internal class CompleteWorklistItemBySignedReportCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<CompleteWorklistItemBySignedReportCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task Handle(CompleteWorklistItemBySignedReportCommand command, CancellationToken cancellationToken)
    {
        WorklistItem? worklistItem = await _aggregateStore
            .Load<WorklistItem>(new WorklistItemId(command.WorklistItemId))
            .ConfigureAwait(false)
            ?? throw new System.InvalidOperationException($"WorklistItem '{command.WorklistItemId}' not found");

        worklistItem.Complete(command.CompletedAt);
        _aggregateStore.AppendChanges(worklistItem);
    }
}
