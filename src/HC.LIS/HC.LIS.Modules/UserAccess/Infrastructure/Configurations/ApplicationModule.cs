using Autofac;
using HC.LIS.Modules.UserAccess.Application.Configuration.Queries;
using HC.LIS.Modules.UserAccess.Infrastructure.Configurations.Processing;

namespace HC.LIS.Modules.UserAccess.Infrastructure.Configurations;

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
