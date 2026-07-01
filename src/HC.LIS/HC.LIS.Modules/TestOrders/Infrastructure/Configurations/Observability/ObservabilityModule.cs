using System.Diagnostics;
using Autofac;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Observability;

internal class ObservabilityModule : Autofac.Module
{
    private static readonly ActivitySource Source = new("HC.LIS.TestOrders");

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterInstance(Source).SingleInstance();
    }
}
