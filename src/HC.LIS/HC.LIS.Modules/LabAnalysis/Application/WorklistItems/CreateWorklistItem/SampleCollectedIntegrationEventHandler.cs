using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.SampleCollection.IntegrationEvents;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.CreateWorklistItem;

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
        foreach (var exam in notification.Exams)
        {
            await _commandsScheduler.EnqueueAsync(new CreateWorklistItemCommand(
                DeriveWorklistItemId(notification.SampleId, exam.ExamMnemonic),
                notification.SampleId,
                notification.SampleBarcode,
                exam.ExamMnemonic,
                notification.PatientId,
                notification.OccurredAt
            )).ConfigureAwait(false);
        }
    }

    private static Guid DeriveWorklistItemId(Guid sampleId, string examCode)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes($"{sampleId}:{examCode}");
        byte[] hash = SHA256.HashData(inputBytes);
        // Use first 16 bytes and fix version/variant bits for RFC 4122 compatibility
        hash[6] = (byte)((hash[6] & 0x0F) | 0x50); // version 5
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80); // RFC 4122 variant
        return new Guid(hash[..16]);
    }
}
