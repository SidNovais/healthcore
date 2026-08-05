using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Physicians;

public class PhysicianInfo : ValueObject
{
    public string FullName { get; }
    public string? LicenceNumber { get; }

    private PhysicianInfo(string fullName, string? licenceNumber)
    {
        FullName = fullName;
        LicenceNumber = licenceNumber;
    }

    public static PhysicianInfo Of(string fullName, string? licenceNumber = null)
        => new(fullName, licenceNumber);
}
