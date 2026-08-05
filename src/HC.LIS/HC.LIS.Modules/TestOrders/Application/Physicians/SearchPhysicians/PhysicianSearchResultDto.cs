namespace HC.LIS.Modules.TestOrders.Application.Physicians.SearchPhysicians;

public class PhysicianSearchResultDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? LicenceNumber { get; set; }
    public string Status { get; set; } = string.Empty;
}
