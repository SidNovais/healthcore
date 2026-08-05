using System;
using System.Threading.Tasks;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Physicians.GetPhysicianDetails;

namespace HC.LIS.Tests.IntegrationEvents.Probes;

public sealed class GetPhysicianDetailsFromTestOrdersProbe(
    Guid physicianId,
    ITestOrdersModule module
) : IProbe<PhysicianDetailsDto>
{
    public string DescribeFailureTo() =>
        $"PhysicianDetails for {physicianId} not found in TestOrders";

    public async Task<PhysicianDetailsDto?> GetSampleAsync() =>
        await module
            .ExecuteQueryAsync(new GetPhysicianDetailsQuery(physicianId))
            .ConfigureAwait(false);

    public bool IsSatisfied(PhysicianDetailsDto? sample) => sample is not null;
}
