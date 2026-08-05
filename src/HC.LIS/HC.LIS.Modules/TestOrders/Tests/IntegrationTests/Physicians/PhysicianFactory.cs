using System;
using System.Threading.Tasks;
using HC.Core.Domain;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Physicians.RegisterPhysician;

namespace HC.LIS.Modules.TestOrders.IntegrationTests.Physicians;

internal static class PhysicianFactory
{
    public static Task CreateAsync(ITestOrdersModule testOrdersModule) => CreateAsync(
        testOrdersModule,
        PhysicianSampleData.PhysicianId,
        PhysicianSampleData.FullName,
        PhysicianSampleData.LicenceNumber
    );

    public static async Task CreateAsync(
        ITestOrdersModule testOrdersModule,
        Guid physicianId,
        string fullName,
        string? licenceNumber
    )
    {
        await testOrdersModule.ExecuteCommandAsync(
            new RegisterPhysicianCommand(
                physicianId,
                fullName,
                licenceNumber,
                SystemClock.Now
            )
        ).ConfigureAwait(false);
    }
}
