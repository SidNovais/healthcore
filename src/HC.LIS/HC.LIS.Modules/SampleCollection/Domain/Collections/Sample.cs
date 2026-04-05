using System;
using System.Collections.Generic;
using HC.Core.Domain;
using HC.LIS.Modules.SampleCollection.Domain.Collections.Events;
using HC.LIS.Modules.SampleCollection.Domain.Collections.Rules;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections;

public class Sample : Entity
{
    internal SampleId SampleId { get; private set; } = null!;
    internal string TubeType { get; private set; } = null!;
    private readonly List<CollectionExam> _exams = [];
    internal IReadOnlyCollection<CollectionExam> Exams => _exams.AsReadOnly();
    internal bool HasBarcode => !_status.IsPending;
    private string? _barcodeValue;
    internal string? Barcode => _barcodeValue;
    private SampleStatus _status = null!;

    private Sample() { }

    internal static Sample CreateForExam(SampleCreatedForExamDomainEvent domainEvent)
    {
        Sample sample = new();
        sample.Apply(domainEvent);
        return sample;
    }

    internal void AddExam(ExamAddedToExistingSampleDomainEvent domainEvent)
        => Apply(domainEvent);

    internal void AssignBarcode(BarcodeCreatedDomainEvent domainEvent)
        => Apply(domainEvent);

    internal void Collect(SampleCollectedDomainEvent domainEvent)
    {
        CheckRule(new CannotCollectSampleMoreThanOnceRule(_status));
        Apply(domainEvent);
    }

    private void Apply(IDomainEvent domainEvent) => When((dynamic)domainEvent);

    private void When(SampleCreatedForExamDomainEvent domainEvent)
    {
        SampleId = new(domainEvent.SampleId);
        TubeType = domainEvent.TubeType;
        _exams.Add(CollectionExam.Of(domainEvent.ExamId, domainEvent.TubeType, domainEvent.ExamMnemonic));
        _status = SampleStatus.Pending;
    }

    private void When(ExamAddedToExistingSampleDomainEvent domainEvent)
        => _exams.Add(CollectionExam.Of(domainEvent.ExamId, TubeType, domainEvent.ExamMnemonic));

    private void When(BarcodeCreatedDomainEvent domainEvent)
    {
        _barcodeValue = domainEvent.BarcodeValue;
        _status = SampleStatus.BarcodeCreated;
    }

    private void When(SampleCollectedDomainEvent _)
        => _status = SampleStatus.Collected;
}
