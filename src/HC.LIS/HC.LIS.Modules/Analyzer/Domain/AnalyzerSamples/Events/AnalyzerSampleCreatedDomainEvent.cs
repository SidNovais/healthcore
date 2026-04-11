using System;
using System.Collections.Generic;
using HC.Core.Domain;

namespace HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples.Events;

public class AnalyzerSampleCreatedDomainEvent(
    Guid analyzerSampleId,
    Guid sampleId,
    Guid patientId,
    string sampleBarcode,
    string patientName,
    DateTime patientBirthdate,
    string patientGender,
    bool isUrgent,
    IReadOnlyCollection<string> examMnemonics,
    DateTime createdAt
) : DomainEvent
{
    public Guid AnalyzerSampleId { get; } = analyzerSampleId;
    public Guid SampleId { get; } = sampleId;
    public Guid PatientId { get; } = patientId;
    public string SampleBarcode { get; } = sampleBarcode;
    public string PatientName { get; } = patientName;
    public DateTime PatientBirthdate { get; } = patientBirthdate;
    public string PatientGender { get; } = patientGender;
    public bool IsUrgent { get; } = isUrgent;
    public IReadOnlyCollection<string> ExamMnemonics { get; } = examMnemonics;
    public DateTime CreatedAt { get; } = createdAt;
}
