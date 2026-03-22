namespace HC.LIS.Modules.SampleCollection.Application.Collections.GetCollectionRequestDetails;

public class CollectionRequestDetailsDto
{
    public Guid CollectionRequestId { get; set; }
    public Guid PatientId { get; set; }
    public Guid OrderId { get; set; }
    public string Status { get; set; }
    public DateTime ArrivedAt { get; set; }
    public DateTime? WaitingAt { get; set; }
    public DateTime? CalledAt { get; set; }
}
