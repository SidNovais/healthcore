using System.Text;
using HC.Core.Application;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.Analyzer.Application.Configuration.Commands;
using HC.LIS.Modules.Analyzer.Application.Configuration.Queries;
using HC.LIS.Modules.Analyzer.Application.Contracts;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetSampleInfoByBarcode;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.HandleBarcodeQuery;
using HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.ForwardRawResult;

internal class ForwardRawResultCommandHandler(
    IHL7ResultParser hl7ResultParser,
    IQueryHandler<GetSampleInfoByBarcodeQuery, SampleInfoDto?> sampleInfoByBarcodeQuery,
    IAggregateStore aggregateStore
) : ICommandHandler<ForwardRawResultCommand>
{
    private readonly IHL7ResultParser _hl7ResultParser = hl7ResultParser;
    private readonly IQueryHandler<GetSampleInfoByBarcodeQuery, SampleInfoDto?> _sampleInfoByBarcodeQuery = sampleInfoByBarcodeQuery;
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task Handle(ForwardRawResultCommand command, CancellationToken cancellationToken)
    {
        AnalyzerResultDto result = _hl7ResultParser.Parse(
            Encoding.UTF8.GetString(command.RawResultPayload.Span));

        SampleInfoDto? sampleInfo = await _sampleInfoByBarcodeQuery
            .Handle(new GetSampleInfoByBarcodeQuery(result.SampleBarcode), cancellationToken)
            .ConfigureAwait(false);

        if (sampleInfo is null)
            throw new SampleNotFoundException($"No sample found for barcode '{result.SampleBarcode}'");

        AnalyzerSample? sample = await _aggregateStore
            .Load(new AnalyzerSampleId(sampleInfo.Id))
            .ConfigureAwait(false)
            ?? throw new InvalidCommandException($"AnalyzerSample aggregate not found for barcode '{result.SampleBarcode}'");

        sample.ReceiveResult(
            result.ExamMnemonic,
            result.ResultValue,
            result.ResultUnit,
            result.ReferenceRange,
            result.InstrumentId,
            result.RecordedAt);

        _aggregateStore.AppendChanges(sample);
    }
}
