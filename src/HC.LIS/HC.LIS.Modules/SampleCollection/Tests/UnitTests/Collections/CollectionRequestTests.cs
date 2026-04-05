using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HC.LIS.Modules.SampleCollection.Domain.Collections;
using HC.LIS.Modules.SampleCollection.Domain.Collections.Events;
using HC.LIS.Modules.SampleCollection.Domain.Collections.Rules;

namespace HC.LIS.Modules.SampleCollection.UnitTests.Collections;

public class CollectionRequestTests : TestBase
{
    private readonly CollectionRequest _sut;

    public CollectionRequestTests()
    {
        _sut = CollectionRequestFactory.Create();
    }

    [Fact]
    public void CreateIsSuccessful()
    {
        PatientArrivedDomainEvent evt = AssertPublishedDomainEvent<PatientArrivedDomainEvent>(_sut);
        evt.CollectionRequestId.Should().Be(CollectionRequestSampleData.CollectionRequestId);
        evt.PatientId.Should().Be(CollectionRequestSampleData.PatientId);
        evt.ExamPreparationVerified.Should().Be(CollectionRequestSampleData.ExamPreparationVerified);
        evt.ArrivedAt.Should().Be(CollectionRequestSampleData.ArrivedAt);
    }

    [Fact]
    public void AddExamCreatesNewSampleWhenNoSampleExistsForTubeType()
    {
        _sut.AddExam(CollectionRequestSampleData.ExamId1, CollectionRequestSampleData.TubeType, CollectionRequestSampleData.ExamMnemonic1);

        SampleCreatedForExamDomainEvent evt = AssertPublishedDomainEvent<SampleCreatedForExamDomainEvent>(_sut);
        evt.CollectionRequestId.Should().Be(CollectionRequestSampleData.CollectionRequestId);
        evt.SampleId.Should().NotBeEmpty();
        evt.ExamId.Should().Be(CollectionRequestSampleData.ExamId1);
        evt.ExamMnemonic.Should().Be(CollectionRequestSampleData.ExamMnemonic1);
        evt.TubeType.Should().Be(CollectionRequestSampleData.TubeType);
    }

    [Fact]
    public void AddExamGroupsIntoExistingSampleWhenSampleAlreadyExistsForTubeType()
    {
        _sut.AddExam(CollectionRequestSampleData.ExamId1, CollectionRequestSampleData.TubeType, CollectionRequestSampleData.ExamMnemonic1);
        Guid sampleId = _sut.GetDomainEvents()
            .OfType<SampleCreatedForExamDomainEvent>()
            .Single()
            .SampleId;

        _sut.AddExam(CollectionRequestSampleData.ExamId2, CollectionRequestSampleData.TubeType, CollectionRequestSampleData.ExamMnemonic2);

        ExamAddedToExistingSampleDomainEvent evt = AssertPublishedDomainEvent<ExamAddedToExistingSampleDomainEvent>(_sut);
        evt.CollectionRequestId.Should().Be(CollectionRequestSampleData.CollectionRequestId);
        evt.SampleId.Should().Be(sampleId);
        evt.ExamId.Should().Be(CollectionRequestSampleData.ExamId2);
        evt.ExamMnemonic.Should().Be(CollectionRequestSampleData.ExamMnemonic2);
    }

    [Fact]
    public void MoveToWaitingIsSuccessful()
    {
        _sut.MoveToWaiting(CollectionRequestSampleData.WaitingAt);

        PatientWaitingDomainEvent evt = AssertPublishedDomainEvent<PatientWaitingDomainEvent>(_sut);
        evt.CollectionRequestId.Should().Be(CollectionRequestSampleData.CollectionRequestId);
        evt.PatientId.Should().Be(CollectionRequestSampleData.PatientId);
        evt.WaitingAt.Should().Be(CollectionRequestSampleData.WaitingAt);
    }

    [Fact]
    public void CallPatientIsSuccessful()
    {
        _sut.MoveToWaiting(CollectionRequestSampleData.WaitingAt);
        _sut.CallPatient(CollectionRequestSampleData.TechnicianId, CollectionRequestSampleData.CalledAt);

        PatientCalledDomainEvent evt = AssertPublishedDomainEvent<PatientCalledDomainEvent>(_sut);
        evt.CollectionRequestId.Should().Be(CollectionRequestSampleData.CollectionRequestId);
        evt.PatientId.Should().Be(CollectionRequestSampleData.PatientId);
        evt.TechnicianId.Should().Be(CollectionRequestSampleData.TechnicianId);
        evt.CalledAt.Should().Be(CollectionRequestSampleData.CalledAt);
    }

    [Fact]
    public void CreateBarcodeIsSuccessful()
    {
        Guid sampleId = CollectionRequestFactory.AddExams(_sut);
        _sut.MoveToWaiting(CollectionRequestSampleData.WaitingAt);
        _sut.CreateBarcode(
            CollectionRequestSampleData.TubeType,
            CollectionRequestSampleData.BarcodeValue,
            CollectionRequestSampleData.TechnicianId,
            CollectionRequestSampleData.CreatedAt
        );

        BarcodeCreatedDomainEvent evt = AssertPublishedDomainEvent<BarcodeCreatedDomainEvent>(_sut);
        evt.CollectionRequestId.Should().Be(CollectionRequestSampleData.CollectionRequestId);
        evt.SampleId.Should().Be(sampleId);
        evt.PatientId.Should().Be(CollectionRequestSampleData.PatientId);
        evt.BarcodeValue.Should().Be(CollectionRequestSampleData.BarcodeValue);
        evt.TubeType.Should().Be(CollectionRequestSampleData.TubeType);
        evt.TechnicianId.Should().Be(CollectionRequestSampleData.TechnicianId);
        evt.Exams.Select(e => e.ExamId).Should().BeEquivalentTo([CollectionRequestSampleData.ExamId1, CollectionRequestSampleData.ExamId2]);
        evt.Exams.Select(e => e.ExamMnemonic).Should().BeEquivalentTo([CollectionRequestSampleData.ExamMnemonic1, CollectionRequestSampleData.ExamMnemonic2]);
        evt.CreatedAt.Should().Be(CollectionRequestSampleData.CreatedAt);
    }

    [Fact]
    public void RecordCollectionIsSuccessful()
    {
        Guid sampleId = CollectionRequestFactory.AddExams(_sut);
        _sut.MoveToWaiting(CollectionRequestSampleData.WaitingAt);
        _sut.CreateBarcode(
            CollectionRequestSampleData.TubeType,
            CollectionRequestSampleData.BarcodeValue,
            CollectionRequestSampleData.TechnicianId,
            CollectionRequestSampleData.CreatedAt
        );
        _sut.CallPatient(CollectionRequestSampleData.TechnicianId, CollectionRequestSampleData.CalledAt);
        _sut.RecordCollection(sampleId, CollectionRequestSampleData.TechnicianId, CollectionRequestSampleData.CollectedAt);

        SampleCollectedDomainEvent evt = AssertPublishedDomainEvent<SampleCollectedDomainEvent>(_sut);
        evt.CollectionRequestId.Should().Be(CollectionRequestSampleData.CollectionRequestId);
        evt.SampleId.Should().Be(sampleId);
        evt.PatientId.Should().Be(CollectionRequestSampleData.PatientId);
        evt.TechnicianId.Should().Be(CollectionRequestSampleData.TechnicianId);
        evt.CollectedAt.Should().Be(CollectionRequestSampleData.CollectedAt);
    }

    [Fact]
    public void MoveToWaitingShouldBreakExamPreparationMustBeVerifiedToAdvanceRuleWhenExamPrepNotVerified()
    {
        CollectionRequest sut = CollectionRequest.Create(
            CollectionRequestSampleData.CollectionRequestId,
            CollectionRequestSampleData.PatientId,
            examPreparationVerified: false,
            CollectionRequestSampleData.ArrivedAt
        );

        void action() => sut.MoveToWaiting(CollectionRequestSampleData.WaitingAt);

        AssertBrokenRule<ExamPreparationMustBeVerifiedToAdvanceRule>(action);
    }

    [Fact]
    public void MoveToWaitingShouldBreakCannotMoveToWaitingWhenNotArrivedRuleWhenAlreadyWaiting()
    {
        _sut.MoveToWaiting(CollectionRequestSampleData.WaitingAt);

        void action() => _sut.MoveToWaiting(CollectionRequestSampleData.WaitingAt);

        AssertBrokenRule<CannotMoveToWaitingWhenNotArrivedRule>(action);
    }

    [Fact]
    public void CallPatientShouldBreakCannotCallPatientWhenNotWaitingRuleWhenNotWaiting()
    {
        void action() => _sut.CallPatient(CollectionRequestSampleData.TechnicianId, CollectionRequestSampleData.CalledAt);

        AssertBrokenRule<CannotCallPatientWhenNotWaitingRule>(action);
    }

    [Fact]
    public void CreateBarcodeShouldBreakCannotCreateBarcodeBeforePatientIsWaitingRuleWhenArrived()
    {
        CollectionRequestFactory.AddExams(_sut);

        void action() => _sut.CreateBarcode(
            CollectionRequestSampleData.TubeType,
            CollectionRequestSampleData.BarcodeValue,
            CollectionRequestSampleData.TechnicianId,
            CollectionRequestSampleData.CreatedAt
        );

        AssertBrokenRule<CannotCreateBarcodeBeforePatientIsWaitingRule>(action);
    }

    [Fact]
    public void CreateBarcodeShouldBreakCannotCreateBarcodeWhenNoPendingSampleForTubeTypeRuleWhenNoExamsAdded()
    {
        _sut.MoveToWaiting(CollectionRequestSampleData.WaitingAt);

        void action() => _sut.CreateBarcode(
            tubeType: "Citrate",
            CollectionRequestSampleData.BarcodeValue,
            CollectionRequestSampleData.TechnicianId,
            CollectionRequestSampleData.CreatedAt
        );

        AssertBrokenRule<CannotCreateBarcodeWhenNoPendingSampleForTubeTypeRule>(action);
    }

    [Fact]
    public void RecordCollectionShouldBreakCannotCollectSampleBeforePatientIsCalledRuleWhenNotCalled()
    {
        Guid sampleId = CollectionRequestFactory.AddExams(_sut);
        _sut.MoveToWaiting(CollectionRequestSampleData.WaitingAt);
        _sut.CreateBarcode(
            CollectionRequestSampleData.TubeType,
            CollectionRequestSampleData.BarcodeValue,
            CollectionRequestSampleData.TechnicianId,
            CollectionRequestSampleData.CreatedAt
        );

        void action() => _sut.RecordCollection(
            sampleId,
            CollectionRequestSampleData.TechnicianId,
            CollectionRequestSampleData.CollectedAt
        );

        AssertBrokenRule<CannotCollectSampleBeforePatientIsCalledRule>(action);
    }

    [Fact]
    public void RecordCollectionShouldBreakCannotCollectSampleMoreThanOnceRuleWhenAlreadyCollected()
    {
        Guid sampleId = CollectionRequestFactory.AddExams(_sut);
        _sut.MoveToWaiting(CollectionRequestSampleData.WaitingAt);
        _sut.CreateBarcode(
            CollectionRequestSampleData.TubeType,
            CollectionRequestSampleData.BarcodeValue,
            CollectionRequestSampleData.TechnicianId,
            CollectionRequestSampleData.CreatedAt
        );
        _sut.CallPatient(CollectionRequestSampleData.TechnicianId, CollectionRequestSampleData.CalledAt);
        _sut.RecordCollection(sampleId, CollectionRequestSampleData.TechnicianId, CollectionRequestSampleData.CollectedAt);

        void action() => _sut.RecordCollection(
            sampleId,
            CollectionRequestSampleData.TechnicianId,
            CollectionRequestSampleData.CollectedAt
        );

        AssertBrokenRule<CannotCollectSampleMoreThanOnceRule>(action);
    }
}
