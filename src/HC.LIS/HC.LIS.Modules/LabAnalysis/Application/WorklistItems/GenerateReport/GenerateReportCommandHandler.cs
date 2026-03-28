using System.Threading;
using System.Threading.Tasks;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GenerateReport;

internal class GenerateReportCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<GenerateReportCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task Handle(
        GenerateReportCommand command,
        CancellationToken cancellationToken
    )
    {
        WorklistItem? worklistItem = await _aggregateStore
            .Load<WorklistItem>(new WorklistItemId(command.WorklistItemId))
            .ConfigureAwait(false);
        worklistItem!.GenerateReport(command.ReportPath, command.GeneratedAt);
        _aggregateStore.AppendChanges(worklistItem);
    }
}
