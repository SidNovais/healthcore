namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetAnalyzerSampleDetails;

public class AnalyzerSampleDetailsDto
{
    public Guid Id { get; set; }
    public Guid SampleId { get; set; }
    public Guid PatientId { get; set; }
    public string SampleBarcode { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public DateTime PatientBirthdate { get; set; }
    public string PatientGender { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsUrgent { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
