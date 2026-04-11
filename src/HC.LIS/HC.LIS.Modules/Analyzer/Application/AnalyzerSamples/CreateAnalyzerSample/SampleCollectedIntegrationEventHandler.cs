using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.LIS.Modules.Analyzer.Application.Configuration.Commands;
using HC.LIS.Modules.SampleCollection.IntegrationEvents;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.CreateAnalyzerSample;

public class SampleCollectedIntegrationEventNotificationHandler(
    ICommandsScheduler commandsScheduler
) : INotificationHandler<SampleCollectedIntegrationEvent>
{
    private readonly ICommandsScheduler _commandsScheduler = commandsScheduler;

    public async Task Handle(
        SampleCollectedIntegrationEvent notification,
        CancellationToken cancellationToken
    )
    {
        await _commandsScheduler.EnqueueAsync(new CreateAnalyzerSampleBySampleCollectedCommand(
            Guid.NewGuid(),
            notification.SampleId,
            notification.PatientId,
            notification.SampleBarcode,
            notification.PatientName,
            notification.PatientBirthdate,
            notification.PatientGender,
            notification.IsUrgent,
            notification.Exams.Select(e => new ExamInfoDto(e.ExamId, e.ExamMnemonic)).ToList().AsReadOnly(),
            notification.OccurredAt
        )).ConfigureAwait(false);
    }
}
