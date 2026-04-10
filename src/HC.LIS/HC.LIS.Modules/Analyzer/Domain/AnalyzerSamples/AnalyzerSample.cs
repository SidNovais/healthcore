using System;
using System.Collections.Generic;
using System.Linq;
using HC.Core.Domain;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples.Events;
using HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples.Rules;

namespace HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples;

public class AnalyzerSample : AggregateRoot
{
    private Guid _sampleId;
    private string _sampleBarcode = string.Empty;
    private PatientInfo _patientInfo = null!;
    private AnalyzerSampleStatus _status = null!;
    private IList<AnalyzerSampleExam> _exams = [];
    private DateTime _createdAt;

    private AnalyzerSample() { }

    protected override void Apply(IDomainEvent domainEvent) => When((dynamic)domainEvent);

    public static AnalyzerSample Create(
        Guid analyzerSampleId,
        Guid sampleId,
        string sampleBarcode,
        PatientInfo patientInfo,
        IReadOnlyCollection<ExamInfo> exams,
        DateTime createdAt)
    {
        AnalyzerSample sample = new();
        AnalyzerSampleCreatedDomainEvent domainEvent = new(
            analyzerSampleId,
            sampleId,
            patientInfo.PatientId,
            sampleBarcode,
            patientInfo.PatientName,
            patientInfo.PatientBirthdate,
            patientInfo.PatientGender,
            exams.Select(e => e.ExamMnemonic).ToList().AsReadOnly(),
            createdAt);
        sample.Apply(domainEvent);
        sample.AddDomainEvent(domainEvent);
        return sample;
    }

    public void AssignWorklistItem(string examMnemonic, Guid worklistItemId, DateTime assignedAt)
    {
        CheckRule(new ExamMustExistInSampleRule(_exams, examMnemonic));
        WorklistItemAssignedDomainEvent domainEvent = new(Id, examMnemonic, worklistItemId, assignedAt);
        Apply(domainEvent);
        AddDomainEvent(domainEvent);
    }

    public void ReceiveResult(
        string examMnemonic,
        string resultValue,
        string resultUnit,
        string referenceRange,
        Guid instrumentId,
        DateTime recordedAt)
    {
        CheckRule(new CannotReceiveResultForNonDispatchedSampleRule(_status));
        CheckRule(new ExamMustExistInSampleRule(_exams, examMnemonic));

        AnalyzerSampleExam exam = _exams.Single(x => x.ExamMnemonic == examMnemonic);
        bool allReceived = _exams.All(e => e.ResultReceived || e.ExamMnemonic == examMnemonic);

        ExamResultReceivedDomainEvent domainEvent = new(
            Id,
            examMnemonic,
            exam.AnalyzerSampleExamId?.Value ?? Guid.Empty,
            resultValue,
            resultUnit,
            referenceRange,
            instrumentId,
            allReceived,
            recordedAt);
        Apply(domainEvent);
        AddDomainEvent(domainEvent);
    }

    public void DispatchInfo(DateTime dispatchedAt)
    {
        CheckRule(new CannotDispatchInfoForNonAwaitingQuerySampleRule(_status));
        SampleInfoDispatchedDomainEvent domainEvent = new(Id, _sampleBarcode, dispatchedAt);
        Apply(domainEvent);
        AddDomainEvent(domainEvent);
    }

    private void When(AnalyzerSampleCreatedDomainEvent domainEvent)
    {
        Id = domainEvent.AnalyzerSampleId;
        _sampleId = domainEvent.SampleId;
        _sampleBarcode = domainEvent.SampleBarcode;
        _patientInfo = PatientInfo.Of(
            domainEvent.PatientId,
            domainEvent.PatientName,
            domainEvent.PatientBirthdate,
            domainEvent.PatientGender);
        _status = AnalyzerSampleStatus.AwaitingQuery;
        _exams = domainEvent.ExamMnemonics.Select(m => AnalyzerSampleExam.Create(m)).ToList();
        _createdAt = domainEvent.CreatedAt;
    }

    private void When(WorklistItemAssignedDomainEvent domainEvent)
    {
        AnalyzerSampleExam exam = _exams.Single(x => x.ExamMnemonic == domainEvent.ExamMnemonic);
        exam.AssignId(domainEvent.WorklistItemId);
    }

    private void When(SampleInfoDispatchedDomainEvent domainEvent)
    {
        _status = AnalyzerSampleStatus.InfoDispatched;
    }

    private void When(ExamResultReceivedDomainEvent domainEvent)
    {
        AnalyzerSampleExam exam = _exams.Single(x => x.ExamMnemonic == domainEvent.ExamMnemonic);
        exam.MarkResultReceived();

        if (domainEvent.AllResultsReceived)
        {
            _status = AnalyzerSampleStatus.ResultReceived;
        }
    }
}
