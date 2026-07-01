using System.Diagnostics;
using Autofac;

namespace HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations.Observability;

internal class ObservabilityModule : Autofac.Module
{
    private static readonly ActivitySource Source = new("HC.LIS.LabAnalysis");

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterInstance(Source).SingleInstance();
    }
}
