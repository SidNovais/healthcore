using HC.Core.Application;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.Analyzer.Application.Configuration.Commands;
using HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.ReceiveExamResult;

internal class ReceiveExamResultCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<ReceiveExamResultCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task Handle(
        ReceiveExamResultCommand command,
        CancellationToken cancellationToken
    )
    {
        AnalyzerSample? sample = await _aggregateStore
            .Load(new AnalyzerSampleId(command.AnalyzerSampleId))
            .ConfigureAwait(false)
            ?? throw new InvalidCommandException("AnalyzerSample must exist to receive an exam result");

        sample.ReceiveResult(
            command.ExamMnemonic,
            command.ResultValue,
            command.ResultUnit,
            command.ReferenceRange,
            command.InstrumentId,
            command.RecordedAt);

        _aggregateStore.AppendChanges(sample);
    }
}
