namespace HC.LIS.Modules.SampleCollection.Application.Collections.GetSamplesByCollectionRequestId;

public class SampleSummaryDto
{
    public Guid Id { get; set; }
    public string TubeType { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string Status { get; set; } = string.Empty;
}
