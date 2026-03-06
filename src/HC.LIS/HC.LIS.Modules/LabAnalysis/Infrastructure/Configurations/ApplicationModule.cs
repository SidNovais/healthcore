using Autofac;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Queries;
using HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations.Processing;

namespace HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations;

internal class ApplicationModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<QueryExecutor>()
          .As<IQueryExecutor>()
          .InstancePerLifetimeScope()
        ;
    }
}
