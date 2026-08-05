using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.UnitTests.Physicians;

public readonly struct PhysicianSampleData
{
    public static readonly Guid PhysicianId = Guid.Parse("019b7d2a-4c11-7a3e-9f52-6c0d84b1e770");
    public static readonly string FullName = "Dr. Ana Lima";
    public static readonly string LicenceNumber = "CRM-SP 123456";
    public static readonly DateTime RegisteredAt = SystemClock.Now;

    public static readonly string UpdatedFullName = "Dr. Ana Lima Souza";
    public static readonly string UpdatedLicenceNumber = "CRM-SP 654321";
}
