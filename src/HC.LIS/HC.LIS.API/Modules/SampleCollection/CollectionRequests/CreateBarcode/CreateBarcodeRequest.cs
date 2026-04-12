namespace HC.LIS.API.Modules.SampleCollection.CollectionRequests.CreateBarcode;

internal sealed record CreateBarcodeRequest(
    string TubeType,
    string BarcodeValue,
    Guid TechnicianId);
