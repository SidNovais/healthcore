namespace HC.LIS.API.Modules.TestOrders.Physicians.UpdatePhysician;

internal sealed record UpdatePhysicianRequest(
    string FullName,
    string? LicenceNumber);
