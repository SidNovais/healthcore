using System.Threading;
using System.Threading.Tasks;
using HC.Core.Application;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetSampleInfoByBarcode;
using HC.LIS.Modules.Analyzer.Application.Configuration.Commands;
using HC.LIS.Modules.Analyzer.Application.Configuration.Queries;
using HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.AssignWorklistItem;

internal class AssignWorklistItemByBarcodeAndExamCodeCommandHandler(
    IAggregateStore aggregateStore,
    IQueryHandler<GetSampleInfoByBarcodeQuery, SampleInfoDto?> sampleInfoQueryHandler
) : ICommandHandler<AssignWorklistItemByBarcodeAndExamCodeCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;
    private readonly IQueryHandler<GetSampleInfoByBarcodeQuery, SampleInfoDto?> _sampleInfoQueryHandler = sampleInfoQueryHandler;

    public async Task Handle(
        AssignWorklistItemByBarcodeAndExamCodeCommand command,
        CancellationToken cancellationToken
    )
    {
        SampleInfoDto? dto = await _sampleInfoQueryHandler
            .Handle(new GetSampleInfoByBarcodeQuery(command.SampleBarcode), cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidCommandException($"AnalyzerSample not found for barcode '{command.SampleBarcode}'");

        AnalyzerSample? sample = await _aggregateStore
            .Load(new AnalyzerSampleId(dto.Id))
            .ConfigureAwait(false)
            ?? throw new InvalidCommandException($"AnalyzerSample '{dto.Id}' not found");

        sample.AssignWorklistItem(command.ExamCode, command.WorklistItemId, command.AssignedAt);
        _aggregateStore.AppendChanges(sample);
    }
}
