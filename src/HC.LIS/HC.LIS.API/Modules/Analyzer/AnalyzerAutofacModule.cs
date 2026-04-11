using Autofac;
using HC.LIS.Modules.Analyzer.Application.Contracts;
using HC.LIS.Modules.Analyzer.Infrastructure;

namespace HC.LIS.API.Modules.Analyzer;

internal sealed class AnalyzerAutofacModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<AnalyzerModule>()
            .As<IAnalyzerModule>()
            .InstancePerLifetimeScope();
    }
}
