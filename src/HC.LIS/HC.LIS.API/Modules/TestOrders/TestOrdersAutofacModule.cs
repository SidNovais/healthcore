using Autofac;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Infrastructure;

namespace HC.LIS.API.Modules.TestOrders;

internal sealed class TestOrdersAutofacModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<TestOrdersModule>()
            .As<ITestOrdersModule>()
            .InstancePerLifetimeScope();
    }
}
