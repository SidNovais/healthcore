using System;
using System.Collections.Generic;
using HC.Core.Domain;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Events;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Rules;

namespace HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;

public class WorklistItem : AggregateRoot
{
    private WorklistItemStatus _status = WorklistItemStatus.Pending;
    private Guid _sampleId;
    private string _examCode = string.Empty;
    private readonly List<AnalysisResult> _results = [];

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

    public void RecordResult(
        string analyteCode,
        string resultValue,
        string resultUnit,
        string referenceRange,
        Guid performedById,
        DateTime recordedAt)
    {
        CheckRule(new CannotRecordResultForNonPendingWorklistItemRule(_status));
        AnalysisResult result = AnalysisResult.Create(analyteCode, resultValue, resultUnit, referenceRange);
        CheckRule(new CannotRecordDuplicateAnalyteResultRule(_results.AsReadOnly(), result.AnalyteCode));
        AnalysisResultRecordedDomainEvent domainEvent = new(
            Id,
            analyteCode,
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
        _status = WorklistItemStatus.Pending;
    }

    private void When(AnalysisResultRecordedDomainEvent domainEvent)
    {
        _results.Add(AnalysisResult.Create(
            domainEvent.AnalyteCode,
            domainEvent.ResultValue,
            domainEvent.ResultUnit,
            domainEvent.ReferenceRange));
        if (_status.IsPending)
            _status = WorklistItemStatus.ResultReceived;
    }

    private void When(ReportGeneratedDomainEvent _)
        => _status = WorklistItemStatus.ReportGenerated;

    private void When(WorklistItemCompletedDomainEvent _)
        => _status = WorklistItemStatus.Completed;
}
