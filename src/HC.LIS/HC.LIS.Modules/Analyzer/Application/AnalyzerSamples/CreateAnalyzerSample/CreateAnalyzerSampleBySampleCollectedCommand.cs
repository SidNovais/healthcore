using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using HC.LIS.Modules.Analyzer.Application.Configuration.Commands;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.CreateAnalyzerSample;

[method: JsonConstructor]
public class CreateAnalyzerSampleBySampleCollectedCommand(
    Guid id,
    Guid sampleId,
    Guid patientId,
    string sampleBarcode,
    string patientName,
    DateTime patientBirthdate,
    string patientGender,
    bool isUrgent,
    IReadOnlyCollection<ExamInfoDto> exams,
    DateTime createdAt
) : InternalCommandBase(id)
{
    public Guid SampleId { get; } = sampleId;
    public Guid PatientId { get; } = patientId;
    public string SampleBarcode { get; } = sampleBarcode;
    public string PatientName { get; } = patientName;
    public DateTime PatientBirthdate { get; } = patientBirthdate;
    public string PatientGender { get; } = patientGender;
    public bool IsUrgent { get; } = isUrgent;
    public IReadOnlyCollection<ExamInfoDto> Exams { get; } = exams;
    public DateTime CreatedAt { get; } = createdAt;
}
