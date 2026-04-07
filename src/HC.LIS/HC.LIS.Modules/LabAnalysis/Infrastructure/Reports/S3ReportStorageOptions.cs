namespace HC.LIS.Modules.LabAnalysis.Infrastructure.Reports;

public class S3ReportStorageOptions
{
    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
}
