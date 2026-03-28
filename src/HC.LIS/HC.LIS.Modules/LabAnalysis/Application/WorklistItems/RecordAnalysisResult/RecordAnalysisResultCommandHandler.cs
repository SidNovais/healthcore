using System.Threading;
using System.Threading.Tasks;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.RecordAnalysisResult;

internal class RecordAnalysisResultCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<RecordAnalysisResultCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task Handle(
        RecordAnalysisResultCommand command,
        CancellationToken cancellationToken
    )
    {
        WorklistItem? worklistItem = await _aggregateStore
            .Load<WorklistItem>(new WorklistItemId(command.WorklistItemId))
            .ConfigureAwait(false);
        worklistItem!.RecordResult(command.ResultValue, command.AnalystId, command.RecordedAt);
        _aggregateStore.AppendChanges(worklistItem);
    }
}
