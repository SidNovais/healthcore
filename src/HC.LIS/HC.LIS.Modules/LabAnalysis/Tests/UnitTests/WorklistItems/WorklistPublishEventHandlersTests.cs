using System;
using System.Threading;
using System.Threading.Tasks;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.LabAnalysis.Application.Patients;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.CreateWorklistItem;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GenerateReport;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.RecordAnalysisResult;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Events;
using HC.LIS.Modules.LabAnalysis.IntegrationEvents;
using NSubstitute;

namespace HC.LIS.Modules.LabAnalysis.UnitTests.WorklistItems;

public sealed class WorklistPublishEventHandlersTests
{
    private readonly IEventsBus _bus = Substitute.For<IEventsBus>();
    private readonly Guid _itemId = Guid.NewGuid();
    private readonly Guid _patientId = Guid.NewGuid();

    [Fact]
    public async Task CreatedHandlerEnrichesTheEventWithThePatientSnapshot()
    {
        var patients = Substitute.For<IPatientSnapshotRepository>();
        patients.GetByIdAsync(_patientId).Returns(
            new PatientSnapshotView("Ana Souza", new DateTime(1990, 1, 1), "Female"));
        var sut = new WorklistItemCreatedPublishEventNotificationHandler(_bus, patients);

        await sut.Handle(
            new WorklistItemCreatedNotification(
                new WorklistItemCreatedDomainEvent(
                    _itemId, Guid.NewGuid(), "BC-1", "HGB", _patientId, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow),
                Guid.NewGuid()),
            CancellationToken.None).ConfigureAwait(true);

        await _bus.Received(1).Publish(Arg.Is<WorklistItemCreatedIntegrationEvent>(
            e => e.WorklistItemId == _itemId && e.PatientName == "Ana Souza" && e.PatientGender == "Female"))
            .ConfigureAwait(true);
    }

    [Fact]
    public async Task ResultRecordedHandlerPublishesIntegrationEventWithWorklistItemId()
    {
        var sut = new AnalysisResultRecordedPublishEventNotificationHandler(_bus);

        await sut.Handle(
            new AnalysisResultRecordedNotification(
                new AnalysisResultRecordedDomainEvent(_itemId, "HGB", "13.5", "g/dL", "12-16", Guid.NewGuid(), DateTime.UtcNow),
                Guid.NewGuid()),
            CancellationToken.None).ConfigureAwait(true);

        await _bus.Received(1).Publish(Arg.Is<WorklistItemResultRecordedIntegrationEvent>(e => e.WorklistItemId == _itemId))
            .ConfigureAwait(true);
    }

    [Fact]
    public async Task ReportGeneratedHandlerPublishesIntegrationEventWithWorklistItemId()
    {
        var sut = new ReportGeneratedPublishEventNotificationHandler(_bus);

        await sut.Handle(
            new ReportGeneratedNotification(
                new ReportGeneratedDomainEvent(_itemId, "/reports/1.pdf", DateTime.UtcNow), Guid.NewGuid()),
            CancellationToken.None).ConfigureAwait(true);

        await _bus.Received(1).Publish(Arg.Is<WorklistItemReportGeneratedIntegrationEvent>(e => e.WorklistItemId == _itemId))
            .ConfigureAwait(true);
    }
}
