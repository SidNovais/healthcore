using System;
using System.Threading.Tasks;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Physicians.GetPhysicianDetails;

namespace HC.LIS.Modules.TestOrders.IntegrationTests.Physicians;

public class GetPhysicianDetailsFromTestOrdersProbe(
    Guid expectedPhysicianId,
    ITestOrdersModule testOrdersModule,
    Func<PhysicianDetailsDto?, bool>? satisfiedWhen = null
) : IProbe<PhysicianDetailsDto>
{
    private readonly Guid _expectedPhysicianId = expectedPhysicianId;
    private readonly ITestOrdersModule _testOrdersModule = testOrdersModule;
    private readonly Func<PhysicianDetailsDto?, bool> _satisfiedWhen = satisfiedWhen ?? (dto => dto is not null);

    public string DescribeFailureTo() =>
        $"PhysicianDetails not found or unsatisfied for {_expectedPhysicianId}";

    public async Task<PhysicianDetailsDto?> GetSampleAsync()
    {
        return await _testOrdersModule
            .ExecuteQueryAsync(new GetPhysicianDetailsQuery(_expectedPhysicianId))
            .ConfigureAwait(false);
    }

    public bool IsSatisfied(PhysicianDetailsDto? sample) => _satisfiedWhen(sample);
}
