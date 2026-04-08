using Autofac;
using HC.LIS.Modules.Analyzer.Application.Configuration.Queries;
using HC.LIS.Modules.Analyzer.Infrastructure.Configurations.Processing;

namespace HC.LIS.Modules.Analyzer.Infrastructure.Configurations;

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
