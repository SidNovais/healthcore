using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HC.Core.Domain;
using HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples;
using HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples.Events;
using HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples.Rules;

namespace HC.LIS.Modules.Analyzer.UnitTests.AnalyzerSamples;

public class AnalyzerSampleTests : TestBase
{
    [Fact]
    public void CreateAnalyzerSampleIsSuccessful()
    {
        AnalyzerSample sut = AnalyzerSampleFactory.Create();

        AnalyzerSampleCreatedDomainEvent domainEvent = AssertPublishedDomainEvent<AnalyzerSampleCreatedDomainEvent>(sut);
        domainEvent.AnalyzerSampleId.Should().Be(AnalyzerSampleSampleData.AnalyzerSampleId);
        domainEvent.SampleId.Should().Be(AnalyzerSampleSampleData.SampleId);
        domainEvent.PatientId.Should().Be(AnalyzerSampleSampleData.PatientId);
        domainEvent.SampleBarcode.Should().Be(AnalyzerSampleSampleData.SampleBarcode);
        domainEvent.PatientName.Should().Be(AnalyzerSampleSampleData.PatientName);
        domainEvent.PatientBirthdate.Should().Be(AnalyzerSampleSampleData.PatientBirthdate);
        domainEvent.PatientGender.Should().Be(AnalyzerSampleSampleData.PatientGender);
        domainEvent.ExamMnemonics.Should().BeEquivalentTo(new[] { AnalyzerSampleSampleData.ExamMnemonic, AnalyzerSampleSampleData.ExamMnemonic2 });
        domainEvent.CreatedAt.Should().Be(AnalyzerSampleSampleData.CreatedAt);
    }

    [Fact]
    public void AssignWorklistItemIsSuccessful()
    {
        AnalyzerSample sut = AnalyzerSampleFactory.Create();
        DateTime assignedAt = SystemClock.Now;

        sut.AssignWorklistItem(
            AnalyzerSampleSampleData.ExamMnemonic,
            AnalyzerSampleSampleData.WorklistItemId,
            assignedAt);

        WorklistItemAssignedDomainEvent domainEvent = AssertPublishedDomainEvent<WorklistItemAssignedDomainEvent>(sut);
        domainEvent.AnalyzerSampleId.Should().Be(AnalyzerSampleSampleData.AnalyzerSampleId);
        domainEvent.ExamMnemonic.Should().Be(AnalyzerSampleSampleData.ExamMnemonic);
        domainEvent.WorklistItemId.Should().Be(AnalyzerSampleSampleData.WorklistItemId);
        domainEvent.AssignedAt.Should().Be(assignedAt);
    }

    [Fact]
    public void AssignWorklistItemShouldBrokeExamMustExistInSampleRuleWhenExamDoesNotExist()
    {
        AnalyzerSample sut = AnalyzerSampleFactory.Create();
        DateTime assignedAt = SystemClock.Now;

        void action()
        {
            sut.AssignWorklistItem(
                "NONEXISTENT",
                AnalyzerSampleSampleData.WorklistItemId,
                assignedAt);
        }

        AssertBrokenRule<ExamMustExistInSampleRule>(action);
    }

    [Fact]
    public void DispatchInfoIsSuccessful()
    {
        AnalyzerSample sut = AnalyzerSampleFactory.Create();
        DateTime dispatchedAt = SystemClock.Now;

        sut.DispatchInfo(dispatchedAt);

        SampleInfoDispatchedDomainEvent domainEvent = AssertPublishedDomainEvent<SampleInfoDispatchedDomainEvent>(sut);
        domainEvent.AnalyzerSampleId.Should().Be(AnalyzerSampleSampleData.AnalyzerSampleId);
        domainEvent.SampleBarcode.Should().Be(AnalyzerSampleSampleData.SampleBarcode);
        domainEvent.DispatchedAt.Should().Be(dispatchedAt);
    }

    [Fact]
    public void DispatchInfoShouldBrokeCannotDispatchInfoForNonAwaitingQuerySampleRuleWhenNotAwaitingQuery()
    {
        AnalyzerSample sut = AnalyzerSampleFactory.Create();
        DateTime dispatchedAt = SystemClock.Now;
        sut.DispatchInfo(dispatchedAt);

        void action()
        {
            sut.DispatchInfo(dispatchedAt);
        }

        AssertBrokenRule<CannotDispatchInfoForNonAwaitingQuerySampleRule>(action);
    }

    [Fact]
    public void ReceiveExamResultIsSuccessful()
    {
        AnalyzerSample sut = AnalyzerSampleFactory.CreateWithInfoDispatched();
        sut.AssignWorklistItem(
            AnalyzerSampleSampleData.ExamMnemonic,
            AnalyzerSampleSampleData.WorklistItemId,
            SystemClock.Now);
        DateTime recordedAt = SystemClock.Now;

        sut.ReceiveResult(
            AnalyzerSampleSampleData.ExamMnemonic,
            AnalyzerSampleSampleData.ResultValue,
            AnalyzerSampleSampleData.ResultUnit,
            AnalyzerSampleSampleData.ReferenceRange,
            AnalyzerSampleSampleData.InstrumentId,
            recordedAt);

        ExamResultReceivedDomainEvent domainEvent = AssertPublishedDomainEvent<ExamResultReceivedDomainEvent>(sut);
        domainEvent.AnalyzerSampleId.Should().Be(AnalyzerSampleSampleData.AnalyzerSampleId);
        domainEvent.ExamMnemonic.Should().Be(AnalyzerSampleSampleData.ExamMnemonic);
        domainEvent.WorklistItemId.Should().Be(AnalyzerSampleSampleData.WorklistItemId);
        domainEvent.ResultValue.Should().Be(AnalyzerSampleSampleData.ResultValue);
        domainEvent.ResultUnit.Should().Be(AnalyzerSampleSampleData.ResultUnit);
        domainEvent.ReferenceRange.Should().Be(AnalyzerSampleSampleData.ReferenceRange);
        domainEvent.InstrumentId.Should().Be(AnalyzerSampleSampleData.InstrumentId);
        domainEvent.AllResultsReceived.Should().BeFalse();
        domainEvent.RecordedAt.Should().Be(recordedAt);
    }

    [Fact]
    public void ReceiveLastExamResultSetsAllResultsReceivedTrue()
    {
        AnalyzerSample sut = AnalyzerSampleFactory.CreateWithInfoDispatched();
        sut.AssignWorklistItem(
            AnalyzerSampleSampleData.ExamMnemonic,
            AnalyzerSampleSampleData.WorklistItemId,
            SystemClock.Now);
        sut.AssignWorklistItem(
            AnalyzerSampleSampleData.ExamMnemonic2,
            AnalyzerSampleSampleData.WorklistItemId2,
            SystemClock.Now);
        DateTime recordedAt = SystemClock.Now;

        sut.ReceiveResult(
            AnalyzerSampleSampleData.ExamMnemonic,
            AnalyzerSampleSampleData.ResultValue,
            AnalyzerSampleSampleData.ResultUnit,
            AnalyzerSampleSampleData.ReferenceRange,
            AnalyzerSampleSampleData.InstrumentId,
            recordedAt);

        sut.ReceiveResult(
            AnalyzerSampleSampleData.ExamMnemonic2,
            AnalyzerSampleSampleData.ResultValue,
            AnalyzerSampleSampleData.ResultUnit,
            AnalyzerSampleSampleData.ReferenceRange,
            AnalyzerSampleSampleData.InstrumentId,
            recordedAt);

        IList<ExamResultReceivedDomainEvent> domainEvents = AssertPublishedDomainEvents<ExamResultReceivedDomainEvent>(sut);
        domainEvents.Should().HaveCount(2);
        domainEvents.Last().AllResultsReceived.Should().BeTrue();
    }

    [Fact]
    public void ReceiveExamResultShouldBrokeCannotReceiveResultForNonDispatchedSampleRuleWhenNotDispatched()
    {
        AnalyzerSample sut = AnalyzerSampleFactory.Create();
        DateTime recordedAt = SystemClock.Now;

        void action()
        {
            sut.ReceiveResult(
                AnalyzerSampleSampleData.ExamMnemonic,
                AnalyzerSampleSampleData.ResultValue,
                AnalyzerSampleSampleData.ResultUnit,
                AnalyzerSampleSampleData.ReferenceRange,
                AnalyzerSampleSampleData.InstrumentId,
                recordedAt);
        }

        AssertBrokenRule<CannotReceiveResultForNonDispatchedSampleRule>(action);
    }

    [Fact]
    public void ReceiveExamResultShouldBrokeExamMustExistInSampleRuleWhenExamDoesNotExist()
    {
        AnalyzerSample sut = AnalyzerSampleFactory.CreateWithInfoDispatched();
        DateTime recordedAt = SystemClock.Now;

        void action()
        {
            sut.ReceiveResult(
                "NONEXISTENT",
                AnalyzerSampleSampleData.ResultValue,
                AnalyzerSampleSampleData.ResultUnit,
                AnalyzerSampleSampleData.ReferenceRange,
                AnalyzerSampleSampleData.InstrumentId,
                recordedAt);
        }

        AssertBrokenRule<ExamMustExistInSampleRule>(action);
    }
}
