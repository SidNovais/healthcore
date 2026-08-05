using HC.LIS.Modules.TestOrders.Domain.Physicians;

namespace HC.LIS.Modules.TestOrders.UnitTests.Physicians;

internal static class PhysicianFactory
{
    public static Physician Create() => Physician.Register(
        PhysicianSampleData.PhysicianId,
        PhysicianSampleData.FullName,
        PhysicianSampleData.LicenceNumber,
        PhysicianSampleData.RegisteredAt
    );
}
