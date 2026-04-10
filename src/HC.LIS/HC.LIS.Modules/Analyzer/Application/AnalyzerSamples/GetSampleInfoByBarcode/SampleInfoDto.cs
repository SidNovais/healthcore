namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetSampleInfoByBarcode;

public class SampleInfoDto
{
    public Guid Id { get; set; }
    public string SampleBarcode { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public DateTime PatientBirthdate { get; set; }
    public string PatientGender { get; set; } = string.Empty;
    public IReadOnlyCollection<SampleExamInfoDto> Exams { get; set; } = [];
}

public class SampleExamInfoDto
{
    public string ExamMnemonic { get; set; } = string.Empty;
    public Guid? WorklistItemId { get; set; }
}
