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
        evt.CreatedAt.Should().Be(WorklistItemSampleData.CreatedAt);
    }

    [Fact]
    public void RecordAnalysisResultIsSuccessful()
    {
        WorklistItem item = WorklistItemFactory.CreateWithResult();

        AnalysisResultRecordedDomainEvent evt = AssertPublishedDomainEvent<AnalysisResultRecordedDomainEvent>(item);
        evt.WorklistItemId.Should().Be(WorklistItemSampleData.WorklistItemId);
        evt.ResultValue.Should().Be(WorklistItemSampleData.ResultValue);
        evt.ResultUnit.Should().Be(WorklistItemSampleData.ResultUnit);
        evt.ReferenceRange.Should().Be(WorklistItemSampleData.ReferenceRange);
        evt.PerformedById.Should().Be(WorklistItemSampleData.PerformedById);
        evt.RecordedAt.Should().Be(WorklistItemSampleData.RecordedAt);
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
        evt.CompletedAt.Should().Be(WorklistItemSampleData.CompletedAt);
    }

    [Fact]
    public void RecordResultThrowsWhenNotPending()
    {
        WorklistItem item = WorklistItemFactory.CreateWithResult();

        AssertBrokenRule<CannotRecordResultForNonPendingWorklistItemRule>(() =>
            item.RecordResult(WorklistItemSampleData.ResultValue, WorklistItemSampleData.ResultUnit, WorklistItemSampleData.ReferenceRange, WorklistItemSampleData.PerformedById, WorklistItemSampleData.RecordedAt));
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
