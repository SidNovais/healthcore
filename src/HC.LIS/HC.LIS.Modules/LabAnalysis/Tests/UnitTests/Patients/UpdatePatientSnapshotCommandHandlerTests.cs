using System;
using System.Threading;
using System.Threading.Tasks;
using HC.LIS.Modules.LabAnalysis.Application.Patients;
using HC.LIS.Modules.LabAnalysis.Application.Patients.UpdatePatientSnapshot;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.PatientManagement.IntegrationEvents;
using NSubstitute;
using Xunit;

namespace HC.LIS.Modules.LabAnalysis.UnitTests.Patients;

public class UpdatePatientSnapshotCommandHandlerTests
{
    private static readonly Guid PatientId = Guid.Parse("019b664c-52a4-7f37-a794-000000000002");
    private static readonly DateTime DateOfBirth = new(1985, 3, 20, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime OccurredAt = new(2026, 6, 10, 13, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task HandlerUpdatesSnapshotWithAllFields()
    {
        IPatientSnapshotRepository repo = Substitute.For<IPatientSnapshotRepository>();
        var handler = new UpdatePatientSnapshotByPatientIdCommandHandler(repo);
        var command = new UpdatePatientSnapshotByPatientIdCommand(
            Guid.CreateVersion7(),
            PatientId,
            "Jane Smith",
            DateOfBirth,
            "Female",
            null,
            "DOC456",
            null,
            "jane@example.com"
        );

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        await repo.Received(1).UpdateAsync(
            PatientId,
            "Jane Smith",
            DateOfBirth,
            "Female",
            null,
            "DOC456",
            null,
            "jane@example.com"
        ).ConfigureAwait(true);
    }

    [Fact]
    public async Task IntegrationEventHandlerEnqueuesUpdateCommand()
    {
        ICommandsScheduler scheduler = Substitute.For<ICommandsScheduler>();
        var handler = new PatientUpdatedIntegrationEventNotificationHandler(scheduler);
        var notification = new PatientUpdatedIntegrationEvent(
            Guid.CreateVersion7(),
            OccurredAt,
            PatientId,
            "Jane Smith",
            DateOfBirth,
            "Female",
            null,
            "DOC456",
            null,
            "jane@example.com",
            OccurredAt
        );

        await handler.Handle(notification, CancellationToken.None).ConfigureAwait(true);

        await scheduler.Received(1)
            .EnqueueAsync(Arg.Is<UpdatePatientSnapshotByPatientIdCommand>(c =>
                c.PatientId == PatientId &&
                c.FullName == "Jane Smith" &&
                c.DateOfBirth == DateOfBirth &&
                c.Gender == "Female" &&
                c.DocumentId == "DOC456" &&
                c.Email == "jane@example.com"
            )).ConfigureAwait(true);
    }
}
