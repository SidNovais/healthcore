using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HC.Core.Domain;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Queries;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;
using HC.LIS.Modules.LabAnalysis.Application.Reports;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemDetails;
using HC.LIS.Modules.LabAnalysis.Domain.SignedReports;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;

namespace HC.LIS.Modules.LabAnalysis.Application.SignedReports.UploadHtmlReport;

internal class UploadHtmlReportBySignedReportIdCommandHandler(
    IAggregateStore aggregateStore,
    IQueryHandler<GetWorklistItemDetailsQuery, WorklistItemDetailsDto?> worklistItemQueryHandler,
    IReportStorage reportStorage
) : ICommandHandler<UploadHtmlReportBySignedReportIdCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;
    private readonly IQueryHandler<GetWorklistItemDetailsQuery, WorklistItemDetailsDto?> _worklistItemQueryHandler = worklistItemQueryHandler;
    private readonly IReportStorage _reportStorage = reportStorage;

    public async Task Handle(UploadHtmlReportBySignedReportIdCommand command, CancellationToken cancellationToken)
    {
        SignedReport signedReport = await _aggregateStore
            .Load<SignedReport>(new SignedReportId(command.ReportId))
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"SignedReport '{command.ReportId}' not found");

        WorklistItemDetailsDto dto = await _worklistItemQueryHandler
            .Handle(new GetWorklistItemDetailsQuery(command.WorklistItemId), cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"WorklistItem '{command.WorklistItemId}' not found");

        dto.AnalyteResults = signedReport.AnalyteSnapshots
            .Select(s => new AnalyteResultDto
            {
                AnalyteCode = s.AnalyteCode,
                ResultValue = s.ResultValue,
                ResultUnit = s.ResultUnit,
                ReferenceRange = s.ReferenceRange,
                IsOutOfRange = s.IsOutOfRange,
            })
            .ToList()
            .AsReadOnly();

        string html = HtmlReportTemplate.Generate(dto, command.Signature, command.SignedBy, command.SignedAt);
        string htmlPath = await _reportStorage.SaveHtmlReportAsync(command.WorklistItemId, html, cancellationToken)
            .ConfigureAwait(false);

        signedReport.HtmlUploaded(htmlPath, SystemClock.Now);
        _aggregateStore.AppendChanges(signedReport);
    }
}
