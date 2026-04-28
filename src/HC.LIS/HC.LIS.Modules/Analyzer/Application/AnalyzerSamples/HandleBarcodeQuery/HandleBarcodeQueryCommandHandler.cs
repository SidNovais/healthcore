using System.Text;
using HC.Core.Application;
using HC.Core.Domain;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.Analyzer.Application.Configuration.Commands;
using HC.LIS.Modules.Analyzer.Application.Configuration.Queries;
using HC.LIS.Modules.Analyzer.Application.Contracts;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetSampleInfoByBarcode;
using HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.HandleBarcodeQuery;

internal class HandleBarcodeQueryCommandHandler(
    IHL7QueryParser hl7QueryParser,
    IQueryHandler<GetSampleInfoByBarcodeQuery, SampleInfoDto?> sampleInfoByBarcodeQuery,
    IAggregateStore aggregateStore,
    ISampleInfoPresenter sampleInfoPresenter
) : ICommandHandler<HandleBarcodeQueryCommand, byte[]>
{
    private readonly IHL7QueryParser _hl7QueryParser = hl7QueryParser;
    private readonly IQueryHandler<GetSampleInfoByBarcodeQuery, SampleInfoDto?> _sampleInfoByBarcodeQuery = sampleInfoByBarcodeQuery;
    private readonly IAggregateStore _aggregateStore = aggregateStore;
    private readonly ISampleInfoPresenter _sampleInfoPresenter = sampleInfoPresenter;

    public async Task<byte[]> Handle(HandleBarcodeQueryCommand command, CancellationToken cancellationToken)
    {
        string barcode = _hl7QueryParser.ParseBarcode(command.RawQueryPayload.ToArray());

        SampleInfoDto? dto = await _sampleInfoByBarcodeQuery
            .Handle(new GetSampleInfoByBarcodeQuery(barcode), cancellationToken)
            .ConfigureAwait(false);

        if (dto is null)
            throw new SampleNotFoundException($"No sample found for barcode '{barcode}'");

        AnalyzerSample? sample = await _aggregateStore
            .Load(new AnalyzerSampleId(dto.Id))
            .ConfigureAwait(false)
            ?? throw new InvalidCommandException($"AnalyzerSample aggregate not found for barcode '{barcode}'");

        sample.DispatchInfo(SystemClock.Now);
        _aggregateStore.AppendChanges(sample);

        return Encoding.UTF8.GetBytes(_sampleInfoPresenter.Format(dto));
    }
}
