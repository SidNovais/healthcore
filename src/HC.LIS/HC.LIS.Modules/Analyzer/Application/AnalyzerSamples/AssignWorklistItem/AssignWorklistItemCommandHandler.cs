using HC.Core.Application;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.Analyzer.Application.Configuration.Commands;
using HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.AssignWorklistItem;

internal class AssignWorklistItemCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<AssignWorklistItemCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task Handle(
        AssignWorklistItemCommand command,
        CancellationToken cancellationToken
    )
    {
        AnalyzerSample? sample = await _aggregateStore
            .Load(new AnalyzerSampleId(command.AnalyzerSampleId))
            .ConfigureAwait(false)
            ?? throw new InvalidCommandException("AnalyzerSample must exist to assign a worklist item");

        sample.AssignWorklistItem(command.ExamMnemonic, command.WorklistItemId, command.AssignedAt);
        _aggregateStore.AppendChanges(sample);
    }
}
