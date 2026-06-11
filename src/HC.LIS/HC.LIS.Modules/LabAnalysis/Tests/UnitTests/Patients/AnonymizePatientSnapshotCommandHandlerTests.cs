using System;
using System.Threading;
using System.Threading.Tasks;
using HC.LIS.Modules.LabAnalysis.Application.Patients;
using HC.LIS.Modules.LabAnalysis.Application.Patients.AnonymizePatientSnapshot;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.PatientManagement.IntegrationEvents;
using NSubstitute;
using Xunit;

namespace HC.LIS.Modules.LabAnalysis.UnitTests.Patients;

public class AnonymizePatientSnapshotCommandHandlerTests
{
    private static readonly Guid PatientId = Guid.Parse("019b664c-52a4-7f37-a794-000000000003");
    private static readonly DateTime AnonymizedAt = new(2026, 6, 10, 14, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task HandlerAnonymizesSnapshot()
    {
        IPatientSnapshotRepository repo = Substitute.For<IPatientSnapshotRepository>();
        var handler = new AnonymizePatientSnapshotByPatientIdCommandHandler(repo);
        var command = new AnonymizePatientSnapshotByPatientIdCommand(
            Guid.CreateVersion7(),
            PatientId,
            AnonymizedAt
        );

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        await repo.Received(1).AnonymizeAsync(
            PatientId,
            AnonymizedAt
        ).ConfigureAwait(true);
    }

    [Fact]
    public async Task IntegrationEventHandlerEnqueuesAnonymizeCommand()
    {
        ICommandsScheduler scheduler = Substitute.For<ICommandsScheduler>();
        var handler = new PatientAnonymizedIntegrationEventNotificationHandler(scheduler);
        var notification = new PatientAnonymizedIntegrationEvent(
            Guid.CreateVersion7(),
            AnonymizedAt,
            PatientId,
            AnonymizedAt
        );

        await handler.Handle(notification, CancellationToken.None).ConfigureAwait(true);

        await scheduler.Received(1)
            .EnqueueAsync(Arg.Is<AnonymizePatientSnapshotByPatientIdCommand>(c =>
                c.PatientId == PatientId &&
                c.AnonymizedAt == AnonymizedAt
            )).ConfigureAwait(true);
    }
}
