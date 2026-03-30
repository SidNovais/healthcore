using System;
using HC.Core.Domain;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Events;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Rules;

namespace HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;

public class WorklistItem : AggregateRoot
{
    private string _status = string.Empty;
    private Guid _sampleId;
    private string _examCode = string.Empty;

    private WorklistItem() { }

    protected override void Apply(IDomainEvent domainEvent) => When((dynamic)domainEvent);

    public static WorklistItem Create(
        Guid worklistItemId,
        Guid sampleId,
        string sampleBarcode,
        string examCode,
        Guid patientId,
        DateTime createdAt
    )
    {
        WorklistItem worklistItem = new();
        WorklistItemCreatedDomainEvent domainEvent = new(
            worklistItemId,
            sampleId,
            sampleBarcode,
            examCode,
            patientId,
            createdAt
        );
        worklistItem.Apply(domainEvent);
        worklistItem.AddDomainEvent(domainEvent);
        return worklistItem;
    }

    public void RecordResult(string resultValue, string resultUnit, string referenceRange, Guid performedById, DateTime recordedAt)
    {
        CheckRule(new CannotRecordResultForNonPendingWorklistItemRule(_status));
        AnalysisResultRecordedDomainEvent domainEvent = new(
            Id,
            resultValue,
            resultUnit,
            referenceRange,
            performedById,
            recordedAt
        );
        Apply(domainEvent);
        AddDomainEvent(domainEvent);
    }

    public void GenerateReport(string reportPath, DateTime generatedAt)
    {
        CheckRule(new CannotGenerateReportWithoutResultRule(_status));
        ReportGeneratedDomainEvent domainEvent = new(
            Id,
            reportPath,
            generatedAt
        );
        Apply(domainEvent);
        AddDomainEvent(domainEvent);
    }

    public void Complete(DateTime completedAt)
    {
        CheckRule(new CannotCompleteWorklistItemWithoutReportRule(_status));
        WorklistItemCompletedDomainEvent domainEvent = new(
            Id,
            _sampleId,
            _examCode,
            "Complete",
            completedAt
        );
        Apply(domainEvent);
        AddDomainEvent(domainEvent);
    }

    private void When(WorklistItemCreatedDomainEvent domainEvent)
    {
        Id = domainEvent.WorklistItemId;
        _sampleId = domainEvent.SampleId;
        _examCode = domainEvent.ExamCode;
        _status = "Pending";
    }

    private void When(AnalysisResultRecordedDomainEvent _)
        => _status = "ResultReceived";

    private void When(ReportGeneratedDomainEvent _)
        => _status = "ReportGenerated";

    private void When(WorklistItemCompletedDomainEvent _)
        => _status = "Completed";
}
