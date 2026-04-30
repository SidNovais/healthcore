using System.Threading.Tasks;
using HC.LIS.Modules.SampleCollection.Application.Contracts;
using HC.LIS.Modules.SampleCollection.Infrastructure;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Infrastructure;

namespace HC.LIS.Tests.IntegrationEvents.TestOrders;

[Collection("IntegrationTests")]
public abstract class TestBase : HC.LIS.Tests.IntegrationEvents.TestBase
{
    protected ITestOrdersModule TestOrdersModule { get; private set; } = null!;
    protected ISampleCollectionModule SampleCollectionModule { get; private set; } = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        TestOrdersModule = new TestOrdersModule();
        SampleCollectionModule = new SampleCollectionModule();
    }

    public override Task DisposeAsync() => base.DisposeAsync();
}
