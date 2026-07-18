using System;
using System.Threading;
using System.Threading.Tasks;
using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.SampleCollection.Application.Collections.CallPatient;
using HC.LIS.Modules.SampleCollection.Application.Collections.CreateCollectionRequest;
using HC.LIS.Modules.SampleCollection.Application.Collections.MovePatientToWaiting;
using HC.LIS.Modules.SampleCollection.Domain.Collections.Events;
using HC.LIS.Modules.SampleCollection.IntegrationEvents;
using NSubstitute;

namespace HC.LIS.Modules.SampleCollection.UnitTests.Collections;

public sealed class TriagePublishEventHandlersTests
{
    private readonly IEventsBus _bus = Substitute.For<IEventsBus>();
    private readonly Guid _requestId = Guid.NewGuid();
    private readonly Guid _patientId = Guid.NewGuid();

    [Fact]
    public async Task ArrivedHandlerPublishesIntegrationEventWithRequestAndPatient()
    {
        var sut = new PatientArrivedPublishEventNotificationHandler(_bus);

        await sut.Handle(
            new PatientArrivedNotification(
                new PatientArrivedDomainEvent(_requestId, _patientId, true, false, DateTime.UtcNow), Guid.NewGuid()),
            CancellationToken.None).ConfigureAwait(true);

        await _bus.Received(1).Publish(Arg.Is<PatientArrivedIntegrationEvent>(
            e => e.CollectionRequestId == _requestId && e.PatientId == _patientId)).ConfigureAwait(true);
    }

    [Fact]
    public async Task WaitingHandlerPublishesIntegrationEventWithRequestAndPatient()
    {
        var sut = new PatientWaitingPublishEventNotificationHandler(_bus);

        await sut.Handle(
            new PatientWaitingNotification(
                new PatientWaitingDomainEvent(_requestId, _patientId, DateTime.UtcNow), Guid.NewGuid()),
            CancellationToken.None).ConfigureAwait(true);

        await _bus.Received(1).Publish(Arg.Is<PatientWaitingIntegrationEvent>(
            e => e.CollectionRequestId == _requestId && e.PatientId == _patientId)).ConfigureAwait(true);
    }

    [Fact]
    public async Task CalledHandlerPublishesIntegrationEventWithRequestAndPatient()
    {
        var sut = new PatientCalledPublishEventNotificationHandler(_bus);

        await sut.Handle(
            new PatientCalledNotification(
                new PatientCalledDomainEvent(_requestId, _patientId, Guid.NewGuid(), DateTime.UtcNow), Guid.NewGuid()),
            CancellationToken.None).ConfigureAwait(true);

        await _bus.Received(1).Publish(Arg.Is<PatientCalledIntegrationEvent>(
            e => e.CollectionRequestId == _requestId && e.PatientId == _patientId)).ConfigureAwait(true);
    }
}
