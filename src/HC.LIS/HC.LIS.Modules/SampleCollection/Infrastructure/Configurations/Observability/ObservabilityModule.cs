using System.Diagnostics;
using Autofac;

namespace HC.LIS.Modules.SampleCollection.Infrastructure.Configurations.Observability;

internal class ObservabilityModule : Autofac.Module
{
    private static readonly ActivitySource Source = new("HC.LIS.SampleCollection");

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterInstance(Source).SingleInstance();
    }
}
