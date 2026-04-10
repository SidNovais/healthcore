namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetAnalyzerSampleExamDetails;

public class AnalyzerSampleExamDetailsDto
{
    public Guid Id { get; set; }
    public Guid AnalyzerSampleId { get; set; }
    public string ExamMnemonic { get; set; } = string.Empty;
    public Guid? WorklistItemId { get; set; }
    public string? ResultValue { get; set; }
    public string? ResultUnit { get; set; }
    public string? ReferenceRange { get; set; }
    public Guid? InstrumentId { get; set; }
    public DateTime? RecordedAt { get; set; }
}
