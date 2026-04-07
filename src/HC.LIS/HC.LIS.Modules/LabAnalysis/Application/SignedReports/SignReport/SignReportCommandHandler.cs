using System;
using System.Threading;
using System.Threading.Tasks;
using HC.Core.Domain;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.LabAnalysis.Domain.SignedReports;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;

namespace HC.LIS.Modules.LabAnalysis.Application.SignedReports.SignReport;

internal class SignReportCommandHandler(
    IAggregateStore aggregateStore,
    IWorklistItemForSigningProvider worklistItemForSigningProvider
) : ICommandHandler<SignReportCommand, Guid>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;
    private readonly IWorklistItemForSigningProvider _worklistItemForSigningProvider = worklistItemForSigningProvider;

    public async Task<Guid> Handle(SignReportCommand command, CancellationToken cancellationToken)
    {
        WorklistItemForSigning worklistItem = await _worklistItemForSigningProvider
            .GetAsync(command.WorklistItemId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"WorklistItem '{command.WorklistItemId}' not found");

        Guid reportId = Guid.CreateVersion7();
        SignedReport signedReport = SignedReport.Create(
            reportId,
            worklistItem,
            command.Signature,
            command.SignedBy,
            SystemClock.Now);
        _aggregateStore.Start(signedReport);

        return reportId;
    }
}
