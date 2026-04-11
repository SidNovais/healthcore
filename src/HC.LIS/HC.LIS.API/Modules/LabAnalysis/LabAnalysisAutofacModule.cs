using Autofac;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;
using HC.LIS.Modules.LabAnalysis.Infrastructure;

namespace HC.LIS.API.Modules.LabAnalysis;

internal sealed class LabAnalysisAutofacModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<LabAnalysisModule>()
            .As<ILabAnalysisModule>()
            .InstancePerLifetimeScope();
    }
}
