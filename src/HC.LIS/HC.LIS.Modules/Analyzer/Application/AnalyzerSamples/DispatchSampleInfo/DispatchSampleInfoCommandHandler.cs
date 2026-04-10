using HC.Core.Application;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.Analyzer.Application.Configuration.Commands;
using HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.DispatchSampleInfo;

internal class DispatchSampleInfoCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<DispatchSampleInfoCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task Handle(
        DispatchSampleInfoCommand command,
        CancellationToken cancellationToken
    )
    {
        AnalyzerSample? sample = await _aggregateStore
            .Load(new AnalyzerSampleId(command.AnalyzerSampleId))
            .ConfigureAwait(false)
            ?? throw new InvalidCommandException("AnalyzerSample must exist to dispatch sample info");

        sample.DispatchInfo(command.DispatchedAt);
        _aggregateStore.AppendChanges(sample);
    }
}
