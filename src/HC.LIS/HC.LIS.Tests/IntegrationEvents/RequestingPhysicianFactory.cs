using System;
using System.Threading.Tasks;
using HC.Core.Domain;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Physicians.RegisterPhysician;
using HC.LIS.Tests.IntegrationEvents.Probes;

namespace HC.LIS.Tests.IntegrationEvents;

internal static class RequestingPhysicianFactory
{
    public static async Task<Guid> RegisterAsync(ITestOrdersModule testOrdersModule)
    {
        Guid physicianId = Guid.CreateVersion7();

        await testOrdersModule.ExecuteCommandAsync(
            new RegisterPhysicianCommand(
                physicianId,
                "Dr. Ana Lima",
                "CRM-SP 123456",
                SystemClock.Now)).ConfigureAwait(false);

        await IntegrationTestAssert.AssertEventually(
            new GetPhysicianDetailsFromTestOrdersProbe(physicianId, testOrdersModule),
            timeoutMs: 15_000).ConfigureAwait(false);

        return physicianId;
    }
}
