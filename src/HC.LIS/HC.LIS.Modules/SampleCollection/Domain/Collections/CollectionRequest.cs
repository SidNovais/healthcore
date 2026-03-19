using System;
using System.Collections.Generic;
using System.Linq;
using HC.Core.Domain;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.SampleCollection.Domain.Collections.Events;
using HC.LIS.Modules.SampleCollection.Domain.Collections.Rules;
using HC.LIS.Modules.SampleCollection.Domain.Orders;
using HC.LIS.Modules.SampleCollection.Domain.Patients;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections;

public class CollectionRequest : AggregateRoot
{
    private PatientId _patientId = null!;
    private OrderId _orderId = null!;
    private bool _examPreparationVerified;
    private CollectionStatus _status = null!;
    private IList<Sample> _samples = [];

    private CollectionRequest() { }

    protected override void Apply(IDomainEvent domainEvent) => When((dynamic)domainEvent);

    public static CollectionRequest Create(
        Guid id,
        Guid patientId,
        Guid orderId,
        bool examPreparationVerified,
        DateTime arrivedAt
    )
    {
        CollectionRequest collectionRequest = new();
        PatientArrivedDomainEvent patientArrivedDomainEvent = new(
            id,
            patientId,
            orderId,
            examPreparationVerified,
            arrivedAt
        );
        collectionRequest.Apply(patientArrivedDomainEvent);
        collectionRequest.AddDomainEvent(patientArrivedDomainEvent);
        return collectionRequest;
    }

    public void AddExam(Guid examId, string tubeType)
    {
        Sample? pendingSample = _samples.FirstOrDefault(s => s.TubeType == tubeType && !s.HasBarcode);

        if (pendingSample is null)
        {
            SampleCreatedForExamDomainEvent sampleCreatedForExamDomainEvent = new(
                Id,
                Guid.NewGuid(),
                examId,
                tubeType
            );
            Apply(sampleCreatedForExamDomainEvent);
            AddDomainEvent(sampleCreatedForExamDomainEvent);
        }
        else
        {
            ExamAddedToExistingSampleDomainEvent examAddedToExistingSampleDomainEvent = new(
                Id,
                pendingSample.SampleId.Value,
                examId
            );
            Apply(examAddedToExistingSampleDomainEvent);
            AddDomainEvent(examAddedToExistingSampleDomainEvent);
        }
    }

    public void MoveToWaiting(DateTime waitingAt)
    {
        CheckRule(new ExamPreparationMustBeVerifiedToAdvanceRule(_examPreparationVerified));
        CheckRule(new CannotMoveToWaitingWhenNotArrivedRule(_status));
        PatientWaitingDomainEvent patientWaitingDomainEvent = new(
            Id,
            _patientId.Value,
            waitingAt
        );
        Apply(patientWaitingDomainEvent);
        AddDomainEvent(patientWaitingDomainEvent);
    }

    public void CallPatient(Guid technicianId, DateTime calledAt)
    {
        CheckRule(new CannotCallPatientWhenNotWaitingRule(_status));
        PatientCalledDomainEvent patientCalledDomainEvent = new(
            Id,
            _patientId.Value,
            technicianId,
            calledAt
        );
        Apply(patientCalledDomainEvent);
        AddDomainEvent(patientCalledDomainEvent);
    }

    public void CreateBarcode(
        string tubeType,
        string barcodeValue,
        Guid technicianId,
        DateTime createdAt
    )
    {
        CheckRule(new CannotCreateBarcodeBeforePatientIsWaitingRule(_status));

        Sample? pendingSample = _samples.FirstOrDefault(s => s.TubeType == tubeType && !s.HasBarcode);
        CheckRule(new CannotCreateBarcodeWhenNoPendingSampleForTubeTypeRule(pendingSample is not null));

        BarcodeCreatedDomainEvent barcodeCreatedDomainEvent = new(
            Id,
            pendingSample!.SampleId.Value,
            _patientId.Value,
            _orderId.Value,
            barcodeValue,
            tubeType,
            technicianId,
            pendingSample!.ExamIds,
            createdAt
        );
        Apply(barcodeCreatedDomainEvent);
        AddDomainEvent(barcodeCreatedDomainEvent);
    }

    public void RecordCollection(Guid sampleId, Guid technicianId, DateTime collectedAt)
    {
        CheckRule(new CannotCollectSampleBeforePatientIsCalledRule(_status));
        SampleCollectedDomainEvent sampleCollectedDomainEvent = new(
            Id,
            sampleId,
            _patientId.Value,
            technicianId,
            collectedAt
        );
        Apply(sampleCollectedDomainEvent);
        AddDomainEvent(sampleCollectedDomainEvent);
    }

    private void When(PatientArrivedDomainEvent domainEvent)
    {
        Id = domainEvent.CollectionRequestId;
        _patientId = new(domainEvent.PatientId);
        _orderId = new(domainEvent.OrderId);
        _examPreparationVerified = domainEvent.ExamPreparationVerified;
        _status = CollectionStatus.Arrived;
    }

    private void When(PatientWaitingDomainEvent _)
        => _status = CollectionStatus.Waiting;

    private void When(PatientCalledDomainEvent _)
        => _status = CollectionStatus.Called;

    private void When(SampleCreatedForExamDomainEvent domainEvent)
        => _samples.Add(Sample.CreateForExam(domainEvent));

    private void When(ExamAddedToExistingSampleDomainEvent domainEvent)
        => _samples.Single(s => s.SampleId.Value == domainEvent.SampleId).AddExam(domainEvent);

    private void When(BarcodeCreatedDomainEvent domainEvent)
        => _samples.Single(s => s.SampleId.Value == domainEvent.SampleId).AssignBarcode(domainEvent);

    private void When(SampleCollectedDomainEvent domainEvent)
        => _samples.Single(s => s.SampleId.Value == domainEvent.SampleId).Collect(domainEvent);
}
