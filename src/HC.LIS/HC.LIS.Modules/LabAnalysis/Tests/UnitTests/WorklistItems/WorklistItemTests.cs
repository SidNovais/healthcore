using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Events;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Rules;

namespace HC.LIS.Modules.LabAnalysis.UnitTests.WorklistItems;

public class WorklistItemTests : TestBase
{
    [Fact]
    public void CreateWorklistItemIsSuccessful()
    {
        WorklistItem item = WorklistItemFactory.CreatePending();

        WorklistItemCreatedDomainEvent evt = AssertPublishedDomainEvent<WorklistItemCreatedDomainEvent>(item);
        evt.WorklistItemId.Should().Be(WorklistItemSampleData.WorklistItemId);
        evt.SampleId.Should().Be(WorklistItemSampleData.SampleId);
        evt.SampleBarcode.Should().Be(WorklistItemSampleData.SampleBarcode);
        evt.ExamCode.Should().Be(WorklistItemSampleData.ExamCode);
        evt.PatientId.Should().Be(WorklistItemSampleData.PatientId);
        evt.OrderId.Should().Be(WorklistItemSampleData.OrderId);
        evt.OrderItemId.Should().Be(WorklistItemSampleData.OrderItemId);
        evt.CreatedAt.Should().Be(WorklistItemSampleData.CreatedAt);
    }

    [Fact]
    public void RecordAnalysisResultIsSuccessful()
    {
        WorklistItem item = WorklistItemFactory.CreateWithResult();

        AnalysisResultRecordedDomainEvent evt = AssertPublishedDomainEvent<AnalysisResultRecordedDomainEvent>(item);
        evt.WorklistItemId.Should().Be(WorklistItemSampleData.WorklistItemId);
        evt.AnalyteCode.Should().Be(WorklistItemSampleData.AnalyteCode);
        evt.ResultValue.Should().Be(WorklistItemSampleData.ResultValue);
        evt.ResultUnit.Should().Be(WorklistItemSampleData.ResultUnit);
        evt.ReferenceRange.Should().Be(WorklistItemSampleData.ReferenceRange);
        evt.PerformedById.Should().Be(WorklistItemSampleData.PerformedById);
        evt.RecordedAt.Should().Be(WorklistItemSampleData.RecordedAt);
    }

    [Fact]
    public void RecordMultipleAnalyteResultsIsSuccessful()
    {
        WorklistItem item = WorklistItemFactory.CreatePending();
        item.RecordResult("WBC", "7.4", "10^9/L", "4.0-11.0 10^9/L", WorklistItemSampleData.PerformedById, WorklistItemSampleData.RecordedAt);
        item.RecordResult("RBC", "5.1", "10^12/L", "4.5-5.5 10^12/L", WorklistItemSampleData.PerformedById, WorklistItemSampleData.RecordedAt);

        IReadOnlyCollection<AnalysisResultRecordedDomainEvent> events = AssertPublishedDomainEvents<AnalysisResultRecordedDomainEvent>(item);
        events.Should().HaveCount(2);
        events.Should().Contain(e => e.AnalyteCode == "WBC");
        events.Should().Contain(e => e.AnalyteCode == "RBC");
    }

    [Fact]
    public void GenerateReportIsSuccessful()
    {
        WorklistItem item = WorklistItemFactory.CreateWithReport();

        ReportGeneratedDomainEvent evt = AssertPublishedDomainEvent<ReportGeneratedDomainEvent>(item);
        evt.WorklistItemId.Should().Be(WorklistItemSampleData.WorklistItemId);
        evt.ReportPath.Should().Be(WorklistItemSampleData.ReportPath);
        evt.GeneratedAt.Should().Be(WorklistItemSampleData.GeneratedAt);
    }

    [Fact]
    public void CompleteWorklistItemIsSuccessful()
    {
        WorklistItem item = WorklistItemFactory.CreateWithReport();
        item.Complete(WorklistItemSampleData.CompletedAt);

        WorklistItemCompletedDomainEvent evt = AssertPublishedDomainEvent<WorklistItemCompletedDomainEvent>(item);
        evt.WorklistItemId.Should().Be(WorklistItemSampleData.WorklistItemId);
        evt.SampleId.Should().Be(WorklistItemSampleData.SampleId);
        evt.ExamCode.Should().Be(WorklistItemSampleData.ExamCode);
        evt.CompletionType.Should().Be("Complete");
        evt.OrderId.Should().Be(WorklistItemSampleData.OrderId);
        evt.OrderItemId.Should().Be(WorklistItemSampleData.OrderItemId);
        evt.CompletedAt.Should().Be(WorklistItemSampleData.CompletedAt);
    }

    [Fact]
    public void RecordResultThrowsWhenReportAlreadyGenerated()
    {
        WorklistItem item = WorklistItemFactory.CreateWithReport();

        AssertBrokenRule<CannotRecordResultForNonPendingWorklistItemRule>(() =>
            item.RecordResult(WorklistItemSampleData.AnalyteCode, WorklistItemSampleData.ResultValue, WorklistItemSampleData.ResultUnit, WorklistItemSampleData.ReferenceRange, WorklistItemSampleData.PerformedById, WorklistItemSampleData.RecordedAt));
    }

    [Fact]
    public void RecordResultThrowsOnDuplicateAnalyteCode()
    {
        WorklistItem item = WorklistItemFactory.CreateWithResult();

        AssertBrokenRule<CannotRecordDuplicateAnalyteResultRule>(() =>
            item.RecordResult(WorklistItemSampleData.AnalyteCode, WorklistItemSampleData.ResultValue, WorklistItemSampleData.ResultUnit, WorklistItemSampleData.ReferenceRange, WorklistItemSampleData.PerformedById, WorklistItemSampleData.RecordedAt));
    }

    [Fact]
    public void GenerateReportThrowsWhenResultNotReceived()
    {
        WorklistItem item = WorklistItemFactory.CreatePending();

        AssertBrokenRule<CannotGenerateReportWithoutResultRule>(() =>
            item.GenerateReport(WorklistItemSampleData.ReportPath, WorklistItemSampleData.GeneratedAt));
    }

    [Fact]
    public void CompleteThrowsWhenReportNotGenerated()
    {
        WorklistItem item = WorklistItemFactory.CreateWithResult();

        AssertBrokenRule<CannotCompleteWorklistItemWithoutReportRule>(() =>
            item.Complete(WorklistItemSampleData.CompletedAt));
    }
}
