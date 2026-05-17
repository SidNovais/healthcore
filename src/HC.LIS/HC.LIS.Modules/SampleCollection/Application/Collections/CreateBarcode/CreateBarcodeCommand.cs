using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.CreateBarcode;

public class CreateBarcodeCommand(
    Guid collectionRequestId,
    string tubeType,
    string barcodeValue,
    DateTime createdAt
) : CommandBase
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
    public string TubeType { get; } = tubeType;
    public string BarcodeValue { get; } = barcodeValue;
    public DateTime CreatedAt { get; } = createdAt;
}
