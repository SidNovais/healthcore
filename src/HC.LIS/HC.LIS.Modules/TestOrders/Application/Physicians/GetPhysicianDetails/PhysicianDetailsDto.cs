namespace HC.LIS.Modules.TestOrders.Application.Physicians.GetPhysicianDetails;

public class PhysicianDetailsDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? LicenceNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeactivatedAt { get; set; }
}
