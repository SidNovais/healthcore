namespace HC.LIS.Modules.SampleCollection.Application.Collections.GetSampleDetails;

public class SampleDetailsDto
{
    public Guid SampleId { get; set; }
    public Guid CollectionRequestId { get; set; }
    public string TubeType { get; set; }
    public string? Barcode { get; set; }
    public string Status { get; set; }
    public DateTime? CollectedAt { get; set; }
}
