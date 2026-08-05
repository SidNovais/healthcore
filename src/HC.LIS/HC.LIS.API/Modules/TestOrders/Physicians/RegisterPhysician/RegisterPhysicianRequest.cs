namespace HC.LIS.API.Modules.TestOrders.Physicians.RegisterPhysician;

internal sealed record RegisterPhysicianRequest(
    string FullName,
    string? LicenceNumber);
