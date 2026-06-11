using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using HC.LIS.Modules.LabAnalysis.Application.Patients;
using HC.LIS.Modules.LabAnalysis.Application.Patients.StorePatientSnapshot;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;
using HC.LIS.Modules.PatientManagement.IntegrationEvents;
using NSubstitute;
using Xunit;

namespace HC.LIS.Modules.LabAnalysis.UnitTests.Patients;

public class StorePatientSnapshotCommandHandlerTests
{
    private static readonly Guid PatientId = Guid.Parse("019b664c-52a4-7f37-a794-000000000001");
    private static readonly DateTime DateOfBirth = new(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime RegisteredAt = new(2026, 6, 10, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task HandlerStoresSnapshotWithAllFields()
    {
        IPatientSnapshotRepository repo = Substitute.For<IPatientSnapshotRepository>();
        var handler = new StorePatientSnapshotByPatientIdCommandHandler(repo);
        var command = new StorePatientSnapshotByPatientIdCommand(
            Guid.CreateVersion7(),
            PatientId,
            "John Doe",
            DateOfBirth,
            "Male",
            "Jane Doe",
            "DOC123",
            "+1-555-0100",
            "john@example.com",
            RegisteredAt
        );

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        await repo.Received(1).StoreAsync(
            PatientId,
            "John Doe",
            DateOfBirth,
            "Male",
            "Jane Doe",
            "DOC123",
            "+1-555-0100",
            "john@example.com",
            RegisteredAt
        ).ConfigureAwait(true);
    }

    [Fact]
    public async Task IntegrationEventHandlerEnqueuesStoreCommand()
    {
        ICommandsScheduler scheduler = Substitute.For<ICommandsScheduler>();
        var handler = new PatientRegisteredIntegrationEventNotificationHandler(scheduler);
        var notification = new PatientRegisteredIntegrationEvent(
            Guid.CreateVersion7(),
            RegisteredAt,
            PatientId,
            "John Doe",
            DateOfBirth,
            "Male",
            null,
            null,
            null,
            null,
            RegisteredAt
        );

        await handler.Handle(notification, CancellationToken.None).ConfigureAwait(true);

        await scheduler.Received(1)
            .EnqueueAsync(Arg.Is<StorePatientSnapshotByPatientIdCommand>(c =>
                c.PatientId == PatientId &&
                c.FullName == "John Doe" &&
                c.DateOfBirth == DateOfBirth &&
                c.Gender == "Male" &&
                c.RegisteredAt == RegisteredAt
            )).ConfigureAwait(true);
    }
}
