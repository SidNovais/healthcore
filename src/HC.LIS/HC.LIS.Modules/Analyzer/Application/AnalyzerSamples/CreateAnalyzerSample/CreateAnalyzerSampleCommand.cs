using HC.LIS.Modules.Analyzer.Application.Contracts;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.CreateAnalyzerSample;

public class CreateAnalyzerSampleCommand(
    Guid analyzerSampleId,
    Guid sampleId,
    Guid patientId,
    string sampleBarcode,
    string patientName,
    DateTime patientBirthdate,
    string patientGender,
    bool isUrgent,
    IReadOnlyCollection<ExamInfoDto> exams,
    DateTime createdAt
) : CommandBase<Guid>
{
    public Guid AnalyzerSampleId { get; } = analyzerSampleId;
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
