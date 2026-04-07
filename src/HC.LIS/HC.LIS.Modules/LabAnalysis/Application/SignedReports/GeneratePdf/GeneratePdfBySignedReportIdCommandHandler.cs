using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HC.Core.Domain;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Queries;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;
using HC.LIS.Modules.LabAnalysis.Application.SignedReports.GetSignedReportDetails;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemDetails;
using HC.LIS.Modules.LabAnalysis.Domain.SignedReports;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;

namespace HC.LIS.Modules.LabAnalysis.Application.SignedReports.GeneratePdf;

internal class GeneratePdfBySignedReportIdCommandHandler(
    IAggregateStore aggregateStore,
    IQueryHandler<GetSignedReportByWorklistItemIdQuery, SignedReportDetailsDto?> signedReportQueryHandler,
    IQueryHandler<GetWorklistItemDetailsQuery, WorklistItemDetailsDto?> worklistItemQueryHandler,
    IReportStorage reportStorage,
    IPdfGenerator pdfGenerator
) : ICommandHandler<GeneratePdfBySignedReportIdCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;
    private readonly IQueryHandler<GetSignedReportByWorklistItemIdQuery, SignedReportDetailsDto?> _signedReportQueryHandler = signedReportQueryHandler;
    private readonly IQueryHandler<GetWorklistItemDetailsQuery, WorklistItemDetailsDto?> _worklistItemQueryHandler = worklistItemQueryHandler;
    private readonly IReportStorage _reportStorage = reportStorage;
    private readonly IPdfGenerator _pdfGenerator = pdfGenerator;

    public async Task Handle(GeneratePdfBySignedReportIdCommand command, CancellationToken cancellationToken)
    {
        SignedReport signedReport = await _aggregateStore
            .Load<SignedReport>(new SignedReportId(command.ReportId))
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"SignedReport '{command.ReportId}' not found in event store");

        SignedReportDetailsDto signedReportDto = await _signedReportQueryHandler
            .Handle(new GetSignedReportByWorklistItemIdQuery(command.WorklistItemId), cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"SignedReport for WorklistItem '{command.WorklistItemId}' not found");

        WorklistItemDetailsDto worklistItemDto = await _worklistItemQueryHandler
            .Handle(new GetWorklistItemDetailsQuery(command.WorklistItemId), cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"WorklistItem '{command.WorklistItemId}' not found");

        worklistItemDto.AnalyteResults = signedReport.AnalyteSnapshots
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

        byte[] pdfBytes = await _pdfGenerator.GenerateAsync(
            worklistItemDto,
            signedReportDto.Signature,
            signedReportDto.SignedBy,
            signedReportDto.CreatedAt,
            cancellationToken
        ).ConfigureAwait(false);

        string pdfPath = await _reportStorage.SavePdfReportAsync(command.WorklistItemId, pdfBytes, cancellationToken)
            .ConfigureAwait(false);

        signedReport.PdfUploaded(pdfPath, SystemClock.Now);
        _aggregateStore.AppendChanges(signedReport);
    }
}
